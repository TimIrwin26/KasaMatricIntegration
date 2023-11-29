namespace MatricIntegration.Matric
{
    internal class Configuration
    {
        public const string Matric = "Matric";
        public Configuration() { }
        public string Pin { get; set; } = string.Empty;
        public int ApiPort { get; set; } = 50300;
    }
}
