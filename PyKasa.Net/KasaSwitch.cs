using Python.Runtime;

namespace PyKasa.Net
{
    public sealed class KasaSwitch : IDisposable
    {
        private KasaSwitch(string address, string? outlet, int timeout)
        {
            Address = address;
            Outlet = outlet;
            Timeout = timeout;
        }

        public static KasaSwitch Factory(string pythonDll, string address, string? outlet = null, int timeout = 20)
        {
            var kasaSwitch = new KasaSwitch(address, outlet, timeout);
            InitializePython(pythonDll);
            return kasaSwitch;
        }

        public string Address { get; set; }
        public string? Outlet { get; set; }
        public int Timeout { get; set; }

        private static void InitializePython(string pythonDll)
        {
            PythonEnvironment.StartUp(pythonDll);
        }

        public static bool IsSwitchOn(string pythonDll, string address, string? outlet = null, int timeout = 20)
        {
            using var kasaSwitch = Factory(pythonDll, address, outlet, timeout);
            return kasaSwitch.IsOn;
        }

        public bool IsOn
        {
            get
            {
                using (Py.GIL())
                {
                    var device = Device(Py.Import("kasa"));
                    return device.is_on.As<bool>();
                }
            }
        }
        public bool TurnOn() => SwitchDevice(true);
        public bool TurnOff() => SwitchDevice(false);

        private bool SwitchDevice(bool on)
        {
            using (Py.GIL())
            {
                dynamic asyncio = Py.Import("asyncio");
                asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy());
                var runner = asyncio.Runner();
                var device = Device(Py.Import("kasa"), runner);
                runner.run(on ? device.turn_on() : device.turn_off());

                runner.run(device.update());

                return device.is_on;
            }
        }

        private dynamic Device(dynamic kasa, dynamic? runner = null)
        {
            if (runner == null)
            {
                dynamic asyncio = Py.Import("asyncio");
                runner = asyncio.Runner();
            }

            var device = runner.run(kasa.Discover.connect_single(Address, timeout: Timeout));
            runner.run(device.update());

            if (!device.is_strip) return device;
            if (Outlet == null)
                throw new ArgumentNullException(nameof(Outlet));

            return device.get_plug_by_name(Outlet);
        }

        public void Dispose()
        {
            PythonEnvironment.Shutdown();
        }
    }
}
