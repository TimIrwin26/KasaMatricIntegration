using KasaMatricIntegration.Kasa;
using KasaMatricIntegration.Matric;
using Matric.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace KasaMatricIntegration.MatricIntegration
{
    internal class MatricService : BackgroundService
    {
        public static string ApplicationName = "MatricKasaIntegration";

        private readonly IConfiguration _configuration;
        private readonly ILogger<MatricService> _logger;
        private readonly MatricConfig _config = new();

        private Lazy<global::Matric.Integration.Matric> _matricInstance;
        private global::Matric.Integration.Matric MatricInstance => _matricInstance.Value;

        private ConcurrentBag<ClientInfo> _connectedClients = [];
        private const int MatricClientRecheckFrequency = 100;
        private int MatricClientRecheckCountdown;

        private const string PressEvent = "press";
        private const string ReleaseEvent = "Release";
        private Exception? _matricError;

        private IKasaDeviceService _KasaDeviceService;

        public MatricService(IKasaDeviceService kasaDeviceService, IConfiguration configuration, ILogger<MatricService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            configuration.Bind("Matric", _config);
            _matricInstance = new Lazy<global::Matric.Integration.Matric>(AttachMatricApp);
            _KasaDeviceService = kasaDeviceService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_matricError != null) { throw _matricError; }

                    CheckForNewClients();

                    if (!_connectedClients.IsEmpty)
                    {
                        SetMatricState(_config.DeviceConfig.Variables, _config.DeviceConfig.Buttons);
                    }

                    var taskDelay = TimeSpan.FromSeconds(
                        _connectedClients.IsEmpty ?
                        _config.MatricPollingIntervalSeconds :
                        _config.DeviceConfig.PollingIntervalSeconds);

                    await Task.Delay(taskDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }

        private global::Matric.Integration.Matric AttachMatricApp()
        {
            global::Matric.Integration.Matric matricInstance = new global::Matric.Integration.Matric(ApplicationName, _config.Pin, _config.ApiPort);
            _logger.LogDebug("{Message}", "Connected to matric instance");
            matricInstance.OnError += Matric_OnError;
            matricInstance.OnConnectedClientsReceived += Matric_OnConnectedClientsReceived;
            //matricInstance.OnVariablesChanged += MatricInstance_OnVariablesChanged;
            matricInstance.OnControlInteraction += MatricInstance_OnControlInteraction;

            return matricInstance;
        }

        private void MatricInstance_OnControlInteraction(object sender, object data)
        {
            if (_config.DeviceConfig.Buttons.Count == 0) return;
            if (data == null) return;
#pragma warning disable CS8604 // Possible null reference argument.
            var controlData = ControlInteractionData.Parse(data.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            if (controlData == null) return;

            _KasaDeviceService.SwitchDevice(controlData?.MessageData?.ControlId, controlData?.MessageData?.EventName == PressEvent);
        }

        //private void MatricInstance_OnVariablesChanged(object sender, ServerVariablesChangedEventArgs data)
        //{
        //    var kasaVariables = _config.KasaVariables.Select(v => v.Name);
        //    if (!data.ChangedVariables.Intersect(kasaVariables).Any()) return;


        //}

        private void CheckForNewClients()
        {
            // check for new client connections
            MatricClientRecheckCountdown--;
            if (MatricClientRecheckCountdown <= 0)
            {
                MatricInstance.GetConnectedClients();
                MatricClientRecheckCountdown = _connectedClients.IsEmpty ? 1 : MatricClientRecheckFrequency;
            }
        }

        private void SetMatricState(IReadOnlyCollection<KasaVariable> kasaVariables, IReadOnlyCollection<KasaButton> kasaButtons, bool force = false)
        {
            _KasaDeviceService.CheckKasaState(((IEnumerable<KasaItem>)kasaVariables).Union(kasaButtons));

            SetVariables(kasaVariables);

            SetButtons(kasaButtons);
        }

        private void SetVariables(IReadOnlyCollection<KasaVariable> kasaVariables, bool force = false)
        {
            var serverVariables = kasaVariables
               .Where(k => k.Changed || force)
               .Select(k => k.ToServerVariable());

            if (serverVariables.Any())
                MatricInstance.SetVariables(serverVariables.ToList());
        }

        private void SetButtons(IReadOnlyCollection<KasaButton> kasaButtons, bool force = false)
        {
            var buttons = kasaButtons
               .Where(k => k.Changed || force)
               .Select(k => k.ToButtonStateArgs());

            if (!buttons.Any()) return;

            foreach (var client in _connectedClients)
            {
                MatricInstance.SetButtonsVisualState(client.Id, buttons.ToList());
            }
        }


        private void Matric_OnError(Exception ex) => _matricError = ex;

        private void Matric_OnConnectedClientsReceived(object source, List<ClientInfo> clients) => UpdateClientsList(clients);

        public void UpdateClientsList(List<ClientInfo> connectedClients)
        {
            _connectedClients.Clear(); // not the greatest idea from a threaded perspective
            _logger.LogDebug("{Message}", $"Connected with {connectedClients.Count} clients.");

            foreach (var client in connectedClients)
            {
                _connectedClients.Add(client);
                _logger.LogDebug("{Message}", $"Client: {client.Name}");
            }

            if (_connectedClients.IsEmpty) return;

            SetMatricState(_config.DeviceConfig.Variables, _config.DeviceConfig.Buttons, true);
        }
    }
}
