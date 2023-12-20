using KasaMatricIntegration.Kasa;
using KasaMatricIntegration.Matric;
using Matric.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PyKasa.Net;
using Python.Runtime;
using System.Collections.Concurrent;

namespace KasaMatricIntegration.MatricIntegration
{
    internal class MatricService : BackgroundService
    {
        public static string ApplicationName = "MatricKasaIntegration";

        private readonly IConfiguration _configuration;
        private readonly MatricConfig _config = new();
        private readonly ILogger<MatricService> _logger;

        private Lazy<global::Matric.Integration.Matric> _matricInstance;
        private global::Matric.Integration.Matric MatricInstance => _matricInstance.Value;

        private ConcurrentBag<ClientInfo> _connectedClients = [];
        private const int MatricClientRecheckFrequency = 100;
        private int MatricClientRecheckCountdown;

        private readonly ConcurrentDictionary<KasaItem, int> _deviceFaults = [];
        public const int MaxFaults = 10;
        private const string PressEvent = "press";
        private const string ReleaseEvent = "Release";
        private Exception? _matricError;

        IEnumerable<string?> KasaVariableNames => _config.KasaVariables.Select(v => v.Name);

        public MatricService(IConfiguration configuration, ILogger<MatricService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            configuration.Bind("Matric", _config);
            _matricInstance = new Lazy<global::Matric.Integration.Matric>(AttachMatricApp);
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
                        SetKasaState(_config.KasaVariables, _config.KasaButtons);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_connectedClients.IsEmpty ? _config.MatricPollingIntervalSeconds : _config.KasaPollingIntervalSeconds), stoppingToken);
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
            if (_config.KasaButtons.Count == 0) return;
            if (data == null) return;
#pragma warning disable CS8604 // Possible null reference argument.
            var controlData = ControlInteractionData.Parse(data.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            if (controlData == null) return;

            var button = _config.KasaButtons
                .Where(b => string.Compare(b.Id, controlData.MessageData?.ControlId, StringComparison.OrdinalIgnoreCase) == 0)
                .FirstOrDefault();

            if (button == null) return;

            using var kasaSwitch = KasaSwitch.Factory(_configuration["PythonDll"] ?? "", button?.DeviceIp ?? "");

            kasaSwitch.SwitchDevice(controlData?.MessageData?.EventName == PressEvent);
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

        private void SetKasaState(IReadOnlyCollection<KasaVariable> kasaVariables, IReadOnlyCollection<KasaButton> kasaButtons)
        {
            CheckKasaState(((IEnumerable<KasaItem>)kasaVariables).Union(kasaButtons));

            SetKasaVariables(kasaVariables);

            SetKasaButtons(kasaButtons);
        }      

        private void SetKasaVariables(IReadOnlyCollection<KasaVariable> kasaVariables)
        {
            var serverVariables = kasaVariables
               .Where(k => k.Changed)
               .Select(k => k.ToServerVariable());

            if (serverVariables.Any())
                MatricInstance.SetVariables(serverVariables.ToList());
        }

        private void SetKasaButtons(IReadOnlyCollection<KasaButton> kasaButtons)
        {
            var buttons = kasaButtons
               .Where(k => k.Changed)
               .Select(k => k.ToButtonStateArgs());

            if (!buttons.Any()) return;

            foreach (var client in _connectedClients)
            {
                MatricInstance.SetButtonsVisualState(client.Id, buttons.ToList());
            }
        }

        private void CheckKasaState(IEnumerable<KasaItem> kasaItems)
        {
            using var kasaSwitch = KasaSwitch.Factory(_configuration["PythonDll"] ?? "", "");

            foreach (var item in kasaItems.Where(k => k != null && !string.IsNullOrEmpty(k.DeviceIp)))
            {
                CheckSwitch(kasaSwitch, item);
            }
        }

        private void CheckSwitch(KasaSwitch kasaSwitch, KasaItem item)
        {
            _logger.LogDebug("Checking switch at {ip}", item.DeviceIp);
            try
            {
                var countdown = _deviceFaults.AddOrUpdate(item, 0, (key, value) => value--);
                if (countdown > 0) return;

#pragma warning disable CS8601 // Possible null reference argument.
                kasaSwitch.Address = item.DeviceIp;
#pragma warning restore CS8601 // Possible null reference argument.
                kasaSwitch.Outlet = item.Outlet;
                item.IsOn = kasaSwitch.IsOn;

                // success
                item.Faults = 0;
                _deviceFaults.Remove(item, out countdown);
            }
            catch (PythonException pe)
            {
                item.Faults++;
                _logger.LogError("Exception #{count} connecting to {device} at {ip}: {exception}", item.Faults, item.Name, item.DeviceIp, pe.Message);
                _deviceFaults.AddOrUpdate(item, item.Faults, (key, value) => 2 ^ Math.Max(MaxFaults, item.Faults));
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

            SetKasaState(_config.KasaVariables, _config.KasaButtons);
        }
    }
}
