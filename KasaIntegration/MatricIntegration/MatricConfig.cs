using KasaMatricIntegration.Kasa;

namespace KasaMatricIntegration.Matric
{
    internal class MatricConfig
    {
        public string Pin { get; set; } = "";
        public int ApiPort { get; set; } = 50300;
        public int KasaPollingIntervalSeconds { get; set; } = 5;
        public int MatricPollingIntervalSeconds { get; set; } = 10;
        public List<KasaVariable> KasaVariables { get; set; } = [];
        public List<KasaButton> KasaButtons { get; set; } = [];
    }
}
