using KasaMatricIntegration.Kasa;
using KasaMatricIntegration.Matric;
using Matric.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PyKasa.Net;

namespace KasaMatricIntegration.MatricIntegration
{
    internal class MatricService : BackgroundService
    {
        public static string ApplicationName = "MatricKasaIntegration";

        private readonly IConfiguration _configuration;
        private readonly MatricConfig _config = new();
        private readonly ILogger<MatricService> _logger;

        private global::Matric.Integration.Matric? _matricInstance;
        private List<ClientInfo> _connectedClients = [];

        private Exception? _matricError;

        public MatricService(IConfiguration configuration, ILogger<MatricService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            configuration.Bind("Matric", _config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //try
                    //{
                    if (_matricError != null) { throw _matricError; }

                    if (_matricInstance == null)
                        AttachMatricApp();

                    if (_matricInstance == null) continue;

                    if (_connectedClients.Count != 0)
                    {
                        SetKasaState(_config.KasaVariables, _config.KasaButtons);
                    }
                    else
                    {
                        _matricInstance?.GetConnectedClients();
                    }
                    //}
                    //catch (SocketException se)
                    //{
                    //    _matricError = null;
                    //    _logger.LogError(se, "{Message}", se.Message);

                    //    Environment.Exit(1);
                    //}

                    await Task.Delay(TimeSpan.FromSeconds(_connectedClients.Count == 0 ? _config.MatricPollingIntervalSeconds : _config.KasaPollingIntervalSeconds), stoppingToken);
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

        private void AttachMatricApp()
        {
            _matricInstance = new global::Matric.Integration.Matric(ApplicationName, _config.Pin, _config.ApiPort);
            _logger.LogDebug("{Message}", "Connected to matric instance");
            _matricInstance.OnError += Matric_OnError;
            _matricInstance.OnConnectedClientsReceived += Matric_OnConnectedClientsReceived;
            // _matricInstance.OnControlInteraction += Matric_OnControlInteraction;
            _matricInstance.OnVariablesChanged += Matric_OnVariablesChanged;
            // _matricInstance.GetConnectedClients();
        }

        private void SetKasaState(IReadOnlyCollection<KasaVariable> kasaVariables, IReadOnlyCollection<KasaButton> kasaButtons)
        {
            CheckKasaState(((IEnumerable<KasaItem>)kasaVariables).Union(kasaButtons));

            var serverVariables = kasaVariables
               .Where(k => k.Changed)
               .Select(k => k.ToServerVariable());

            if (serverVariables.Any())
                _matricInstance?.SetVariables(serverVariables.ToList());

            var buttons = kasaButtons
               .Where(k => k.Changed)
               .Select(k => k.ToButtonStateArgs());

            if (!buttons.Any()) return;

            foreach (var client in _connectedClients)
            {
                _matricInstance?.SetButtonsVisualState(client.Id, buttons.ToList());
            }
        }

        private void CheckKasaState(IEnumerable<KasaItem> kasaItems)
        {
            using var kasaSwitch = KasaSwitch.Factory(_configuration["PythonDll"] ?? "", "");

            foreach (var item in kasaItems.Where(k => !string.IsNullOrEmpty(k.DeviceIp)))
            {
#pragma warning disable CS8601 // Possible null reference argument.
                kasaSwitch.Address = item.DeviceIp;
#pragma warning restore CS8601 // Possible null reference argument.
                kasaSwitch.Outlet = item.Outlet;
                item.IsOn = kasaSwitch.IsOn;
            }
        }

        private void Matric_OnVariablesChanged(object sender, ServerVariablesChangedEventArgs data)
        {
            _logger.LogDebug("Server variables changed");
            foreach (var varName in data.ChangedVariables)
            {
                _logger.LogDebug("{Message}", $"{varName}: {data.Variables[varName].Value}");
                var currentItem = _config.KasaVariables.FirstOrDefault(v => v.Name?.Equals(varName) ?? false);
                if (currentItem == null) continue;

                currentItem.IsOn = (bool)data.Variables[varName].Value;
            }
        }

        private void Matric_OnError(Exception ex) => _matricError = ex;

        //private void Matric_OnControlInteraction(object sender, object data)
        //{
        //    _logger.LogDebug("Control interaction:");
        //    _logger.LogDebug(data.ToString());
        //}

        private void Matric_OnConnectedClientsReceived(object source, List<ClientInfo> clients) => UpdateClientsList(clients);

        public void UpdateClientsList(List<ClientInfo> connectedClients)
        {
            _connectedClients = connectedClients;
            _logger.LogDebug("{Message}", $"Connected with {connectedClients.Count} clients.");

            foreach (var client in connectedClients)
            {
                _logger.LogDebug("{Message}", $"Client: {client.Name}");
            }
        }
    }
}
