using KasaMatricIntegration.Kasa;

namespace KasaMatricIntegration.Matric
{
    internal class MatricConfig
    {
        public string ApplicationName { get; set; } = "Matric Integration";
        public string? Pin { get; set; }
        public int ApiPort { get; set; } = 5300;
        public int ListenerPort { get; set; } = 50310;
        public int MatricPollingIntervalSeconds { get; set; } = 10;
        public KasaDeviceConfig DeviceConfig { get; set; } = new KasaDeviceConfig ();
    }
}
