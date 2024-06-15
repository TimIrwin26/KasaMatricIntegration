using KasaMatricIntegration.Matric;
using Matric.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace KasaMatricIntegration.MatricIntegration
{
    internal class MatricAppWrapper : IMatricAppWrapper
    {
        private readonly MatricConfig _config;
        private readonly Lazy<global::Matric.Integration.Matric> _matricInstance;
        private global::Matric.Integration.Matric MatricInstance => _matricInstance.Value;

        private ILogger<MatricService> _logger;

        private const int MatricClientRecheckFrequency = 100;
        private int MatricClientRecheckCountdown;

        public event IMatricAppWrapper.ConnectedClientsReceivedHandler? OnConnectedClientsReceived;
        public event IMatricAppWrapper.ErrorHandler? OnError;
        public event IMatricAppWrapper.ControlInteraction? OnControlInteraction;
        public event IMatricAppWrapper.VariablesChanged? OnVariablesChanged;

        public ConcurrentBag<ClientInfo> ConnectedClients { get; } = [];

        public MatricAppWrapper(IOptions<MatricConfig> configuration, ILogger<MatricService> logger)
        {
            _config= configuration.Value;
            //var foo = configuration.GetSection("Matric");
            //configuration.GetSection("Matric").Bind(_config);

            _matricInstance = new Lazy<global::Matric.Integration.Matric>(AttachMatricApp, LazyThreadSafetyMode.PublicationOnly);
            _logger = logger;
        }

        private global::Matric.Integration.Matric AttachMatricApp()
        {
            global::Matric.Integration.Matric matricInstance = new global::Matric.Integration.Matric(_config.ApplicationName, _config.Pin, _config.ApiPort, _config.ListenerPort);
            matricInstance.OnError += Matric_OnError;
            matricInstance.OnConnectedClientsReceived += Matric_OnConnectedClientsReceived;
            matricInstance.OnVariablesChanged += MatricInstance_OnVariablesChanged;
            matricInstance.OnControlInteraction += MatricInstance_OnControlInteraction;
            matricInstance.GetConnectedClients();
            return matricInstance;
        }

        private void MatricInstance_OnControlInteraction(object sender, object data)
        {
            OnControlInteraction?.Invoke(sender, data);
        }

        private void MatricInstance_OnVariablesChanged(object sender, ServerVariablesChangedEventArgs data)
        {
            OnVariablesChanged?.Invoke(sender, data);
        }

        public void CheckForNewClients()
        {
            // check for new client connections
            MatricClientRecheckCountdown--;
            if (MatricClientRecheckCountdown <= 0)
            {
                MatricInstance.GetConnectedClients();
                // possible threading issue here if this check occurs before ConnectedClients has been updated?
                MatricClientRecheckCountdown = ConnectedClients.IsEmpty ? 1 : MatricClientRecheckFrequency;
            }
        }
        private void Matric_OnError(Exception ex) => throw ex;

        private void Matric_OnConnectedClientsReceived(object source, List<ClientInfo> clients) => UpdateClientsList(clients);

        public void UpdateClientsList(List<ClientInfo> connectedClients)
        {
            ConnectedClients.Clear(); // not the greatest idea from a threaded perspective
            _logger.LogDebug("{Message}", $"Connected with {connectedClients.Count} clients.");

            foreach (var client in connectedClients)
            {
                ConnectedClients.Add(client);
                _logger.LogDebug("{Message}", $"Client: {client.Name}");
            }

            if (ConnectedClients.IsEmpty) return;

            // SetMatricState(_config.DeviceConfig.Variables, _config.DeviceConfig.Buttons, true);
        }

        public void SetVariables(List<ServerVariable> serverVariables)
        {
            MatricInstance.SetVariables(serverVariables);
        }

        public void SetButtonsVisualState(string id, List<SetButtonsVisualStateArgs> setButtonsVisualStateArgs)
        {
            MatricInstance.SetButtonsVisualState(id, setButtonsVisualStateArgs);
        }
    }
}
