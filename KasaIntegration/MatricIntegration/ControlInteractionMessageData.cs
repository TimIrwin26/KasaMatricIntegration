namespace KasaMatricIntegration.MatricIntegration
{
    public class ControlInteractionMessageData
    {
        public string ControlId { get; set; } = string.Empty;
        public string ControlName { get; set; } = string.Empty;
        public string PageId { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public object? Data { get; set; }
        public string DeckId { get; set; } = string.Empty;
    }
}