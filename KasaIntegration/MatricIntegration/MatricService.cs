using KasaMatricIntegration.Kasa;
using KasaMatricIntegration.Matric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KasaMatricIntegration.MatricIntegration
{
    internal class MatricService : BackgroundService, IDisposable
    {
        public static string ApplicationName = "Kasa Matric Service";

        private readonly ILogger<MatricService> _logger;
        private readonly MatricConfig _config = new();

        private IMatricAppWrapper _matricInstance;

        private const string PressEvent = "press";
        private const string ReleaseEvent = "Release";
        private Exception? _matricError;

        private IKasaDeviceService _KasaDeviceService;

        public MatricService(IKasaDeviceService kasaDeviceService, IMatricAppWrapper matricApp, IConfiguration configuration, ILogger<MatricService> logger)
        {
            _logger = logger;
            configuration.Bind("Matric", _config);
            _matricInstance = matricApp;
            _matricInstance.OnControlInteraction += OnControlInteraction;
            _matricInstance.OnVariablesChanged += OnVariablesChanged;
            _KasaDeviceService = kasaDeviceService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_matricError != null) { throw _matricError; }

                    _matricInstance.CheckForNewClients();

                    if (!_matricInstance.ConnectedClients.IsEmpty)
                    {
                        SetMatricState(_config.DeviceConfig.Variables, _config.DeviceConfig.Buttons);
                    }

                    var taskDelay = TimeSpan.FromSeconds(
                        _matricInstance.ConnectedClients.IsEmpty ?
                        _config.MatricPollingIntervalSeconds :
                        _config.DeviceConfig.PollingIntervalSeconds);

                    await Task.Delay(taskDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }



        private void OnControlInteraction(object sender, object data)
        {
            if (_config.DeviceConfig.Buttons.Count == 0) return;
            if (data == null) return;
#pragma warning disable CS8604 // Possible null reference argument.
            var controlData = ControlInteractionData.Parse(data.ToString());
#pragma warning restore CS8604 // Possible null reference argument.
            if (controlData == null) return;

            _KasaDeviceService.SwitchDevice(controlData?.MessageData?.ControlId, controlData?.MessageData?.EventName == PressEvent);
        }

        private void OnVariablesChanged(object sender, global::Matric.Integration.ServerVariablesChangedEventArgs data)
        {
/*            var kasaVariables = _config.KasaVariables.Select(v => v.Name);
            if (!data.ChangedVariables.Intersect(kasaVariables).Any()) return;*/
        }

        private void SetMatricState(IReadOnlyCollection<KasaVariable> kasaVariables, IReadOnlyCollection<KasaButton> kasaButtons, bool force = false)
        {
            _KasaDeviceService.CheckState(((IEnumerable<KasaItem>)kasaVariables).Union(kasaButtons));

            SetVariables(kasaVariables);
            SetButtons(kasaButtons);
        }

        private void SetVariables(IReadOnlyCollection<KasaVariable> kasaVariables, bool force = false)
        {
            var serverVariables = kasaVariables
               .Where(k => k.Changed || force)
               .Select(k => k.ToServerVariable());

            if (serverVariables.Any())
                _matricInstance.SetVariables(serverVariables.ToList());
        }

        private void SetButtons(IReadOnlyCollection<KasaButton> kasaButtons, bool force = false)
        {
            var buttons = kasaButtons
               .Where(k => k.Changed || force)
               .Select(k => k.ToButtonStateArgs());

            if (!buttons.Any()) return;

            foreach (var client in _matricInstance.ConnectedClients)
            {
                _matricInstance.SetButtonsVisualState(client.Id, buttons.ToList());
            }
        }
    }
}
