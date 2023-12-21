using PyKasa.Net.KasaDeviceException;

namespace PyKasa.Net
{
    public sealed class KasaDevice : IKasaDevice
    {
        internal KasaDevice(string address, string? outlet, int timeout)
        {
            Address = address;
            Outlet = outlet;
            Timeout = timeout;
        }
        
        public string Address { get; set; }
        public string? Outlet { get; set; }
        public int Timeout { get; set; }

        public bool IsOn
        {
            get
            {
                using var environment = KasaCallEnvironment.CreateEnvironment();
                return Device(environment).is_on;
            }
        }

        public bool TurnOn() => SwitchDevice(true);
        public bool TurnOff() => SwitchDevice(false);

        public bool ToggleDevice()
        {
            using var environment = KasaCallEnvironment.CreateEnvironment();
            var device = Device(environment);
            return SwitchDevice(!device.device.is_on, environment, device);
        }

        public bool SwitchDevice(bool on)
        {
            using var environment = KasaCallEnvironment.CreateEnvironment();
            return SwitchDevice(on, environment);
        }

        private bool SwitchDevice(bool on, KasaCallEnvironment environment, dynamic? device = null)
        {
            device ??= Device(environment);

            environment.Runner.run(on ? device.turn_on() : device.turn_off());
            environment.Runner.run(device.update());

            return device.is_on;
        }

        private dynamic Device(KasaCallEnvironment environment)
        {
            if(string.IsNullOrEmpty(Address)) throw new EmptyAddressException();
            var device = environment.Runner.run(environment.Kasa.Discover.connect_single(Address, timeout: Timeout));
            environment.Runner.run(device.update());

            if (!device.is_strip) return device;
            if (Outlet == null)
                throw new EmptyOutletException($"No outlet provided for device at IP {Address}");

            return device.get_plug_by_name(Outlet);
        }
    }
}
