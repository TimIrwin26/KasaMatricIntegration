using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PyKasa.Net;
using Python.Runtime;
using System.Collections.Concurrent;

namespace KasaMatricIntegration.Kasa
{
    public class KasaDeviceService : IKasaDeviceService
    {
        public const int MaxFaults = 10;

        private readonly ILogger<KasaDeviceService> _logger;

        private readonly IConfiguration _configuration;
        private readonly KasaDeviceConfig _config = new();
        private readonly IKasaDeviceFactory _kasaDeviceFactory;

        private readonly ConcurrentDictionary<KasaItem, int> _deviceFaults = [];

        public KasaDeviceService(IKasaDeviceFactory kasaDeviceFactory, IConfiguration configuration, ILogger<KasaDeviceService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            configuration.Bind("Matric:DeviceConfig", _config);
            _kasaDeviceFactory = kasaDeviceFactory;
        }

        IEnumerable<string?> KasaVariableNames => _config.Variables.Select(v => v.Name);

        public void SwitchDevice(string? id, bool on)
        {
            if (id == null) return;
            var button = _config.Buttons
                .Where(b => string.Compare(b.Id, id, StringComparison.OrdinalIgnoreCase) == 0)
                .FirstOrDefault();

            if (button == null) return;

            var kasaDevice = _kasaDeviceFactory.CreateDevice(button?.DeviceIp ?? "");

            kasaDevice.SwitchDevice(on);
        }

        public void CheckKasaState(IEnumerable<KasaItem> kasaItems)
        {
            var kasaDevice = _kasaDeviceFactory.CreateDevice();

            foreach (var item in kasaItems.Where(k => k != null && !string.IsNullOrEmpty(k.DeviceIp)))
            {
                CheckDeviceState(kasaDevice, item);
            }
        }

        public void CheckDeviceState(IKasaDevice kasaDevice, KasaItem item)
        {
            _logger.LogDebug("Checking device at {ip}", item.DeviceIp);
            try
            {
                var countdown = _deviceFaults.AddOrUpdate(item, 0, (key, value) => value--);
                if (countdown > 0) return;

#pragma warning disable CS8601 // Possible null reference argument.
                kasaDevice.Address = item.DeviceIp;
#pragma warning restore CS8601 // Possible null reference argument.
                kasaDevice.Outlet = item.Outlet;
                item.IsOn = kasaDevice.IsOn;

                // success
                item.Faults = 0;
                _deviceFaults.Remove(item, out countdown);
            }
            catch (PythonException pe)
            {
                item.Faults++;
                _logger.LogError("Exception #{count} connecting to {device} at {ip}: {exception}", item.Faults, item.Name, item.DeviceIp, pe.Message);
                _deviceFaults.AddOrUpdate(item, item.Faults, (key, value) => 2 ^ Math.Max(MaxFaults, item.Faults));
            }
        }
    }
}
