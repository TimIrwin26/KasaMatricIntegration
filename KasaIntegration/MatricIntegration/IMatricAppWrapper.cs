using Matric.Integration;
using System.Collections.Concurrent;

namespace KasaMatricIntegration.MatricIntegration
{
    internal interface IMatricAppWrapper
    {
        void CheckForNewClients();
        void UpdateClientsList(List<ClientInfo> connectedClients);
        void SetVariables(List<ServerVariable> serverVariables);
        void SetButtonsVisualState(string id, List<SetButtonsVisualStateArgs> setButtonsVisualStateArgs);

        ConcurrentBag<ClientInfo> ConnectedClients { get; }

        public delegate void ConnectedClientsReceivedHandler(object source, List<ClientInfo> clients);
        public delegate void ErrorHandler(Exception ex);
        public delegate void ControlInteraction(object sender, object data);
        public delegate void VariablesChanged(object sender, ServerVariablesChangedEventArgs data);
        public event ConnectedClientsReceivedHandler? OnConnectedClientsReceived;
        public event ErrorHandler? OnError;
        public event ControlInteraction? OnControlInteraction;
        public event VariablesChanged? OnVariablesChanged;
    }
}