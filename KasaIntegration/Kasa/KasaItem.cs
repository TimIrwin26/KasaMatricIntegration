namespace KasaMatricIntegration.Kasa
{
    internal class KasaItem
    {
        public string? Name { get; set; }
        public string? DeviceIp { get; set; }
        public string? Outlet { get; set; }

        private bool _isOn = false;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                Changed = _isOn != value;
                _isOn = value;

            }
        }

        public bool Changed { get; set; }
    }
}
