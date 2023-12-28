using KasaMatricIntegration.Kasa;

namespace KasaMatricIntegration.Matric
{
    internal class MatricConfig
    {
        public string Pin { get; set; } = "";
        public int ApiPort { get; set; } = 50300;
        public int MatricPollingIntervalSeconds { get; set; } = 10;
        public KasaDeviceConfig DeviceConfig { get; set; } = new KasaDeviceConfig ();
    }
}
