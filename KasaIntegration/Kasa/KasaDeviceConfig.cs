namespace KasaMatricIntegration.Kasa
{
    internal class KasaDeviceConfig
    {
        public int PollingIntervalSeconds { get; set; } = 5;
        public List<KasaVariable> Variables { get; set; } = [];
        public List<KasaButton> Buttons { get; set; } = [];
    }
}