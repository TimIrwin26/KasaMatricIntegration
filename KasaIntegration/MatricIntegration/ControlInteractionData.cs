using Matric.Integration;
using Newtonsoft.Json;

namespace KasaMatricIntegration.MatricIntegration
{
    public class ControlInteractionData
    {
        public const string EventMessageType = "controlevent";
        public ControlInteractionMessageData? MessageData { get; set; } = null;
        public ClientInfo? ClientInfo { get; set; } = null;
        public string MessageType { get; set; } = string.Empty;

        public static ControlInteractionData Parse(string data)
        {
            var controlData = JsonConvert.DeserializeObject<ControlInteractionData>(data?.ToString() ?? string.Empty);
            return controlData ?? new ControlInteractionData();
        }
    }
}
