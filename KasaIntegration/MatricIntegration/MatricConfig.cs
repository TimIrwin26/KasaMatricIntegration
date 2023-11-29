using KasaMatricIntegration.Kasa;

namespace KasaMatricIntegration.Matric
{
    internal class MatricConfig
    {
        public string Pin { get; set; } = "";
        public int ApiPort { get; set; } = 50300;
        public int PollingInterval { get; set; } = 5;
        public List<KasaVariable> KasaVariables { get; set; } = new();
        public List<KasaButton> KasaButtons { get; set; } = new();
    }
}
