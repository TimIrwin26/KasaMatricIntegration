using KasaConsole.Kasa;
using KasaMatricIntegration.Kasa;
using KasaMatricIntegration.Matric;
using Matric.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KasaIntegration
{
    internal class MatricService : BackgroundService
    {
        public static string ApplicationName = "MatricKasaIntegration";

        private readonly IConfiguration Configuration;
        private readonly MatricConfig Config = new MatricConfig();
        private readonly ILogger Logger;

        private Matric.Integration.Matric? MatricInstance;
        private string? ClientId;

        private Exception? MatricError;

        public MatricService(IConfiguration configuration, ILogger<MatricService> logger)
        {
            Configuration = configuration;
            Logger = logger;
            configuration.Bind("Matric", Config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                MatricInstance = new Matric.Integration.Matric(ApplicationName, Configuration["Matric:Pin"], Convert.ToInt32(Configuration["Matric:ApiPort"]));
                MatricInstance.OnError += Matric_OnError;
                MatricInstance.OnConnectedClientsReceived += Matric_OnConnectedClientsReceived;
                MatricInstance.OnControlInteraction += Matric_OnControlInteraction;
                MatricInstance.OnVariablesChanged += Matric_OnVariablesChanged;
                MatricInstance.GetConnectedClients();

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (MatricError != null) { throw MatricError; }
                    if (ClientId != null)
                    {
                        SetKasaState(Config.KasaVariables, Config.KasaButtons);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Message}", ex.Message);

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

        private void SetKasaState(IEnumerable<KasaVariable> kasaVariables, IEnumerable<KasaButton> kasaButtons)
        {
            CheckKasaState(((IEnumerable<KasaItem>)kasaVariables).Union(kasaButtons));

            var serverVariables = kasaVariables
               .Where(k => k.Changed)
               .Select(k => k.ToServerVariable());
            MatricInstance?.SetVariables(serverVariables.ToList());

            var buttons = kasaButtons
               .Where(k => k.Changed)
               .Select(k=>k.ToButtonStateArgs());

            MatricInstance?.SetButtonsVisualState(MatricInstance?.ClientId, buttons.ToList());
        }

        private void CheckKasaState(IEnumerable<KasaItem> kasaItems)
        {
            using var kasaSwitch = KasaSwitch.Factory(Configuration["PythonDll"] ?? "", "");

            foreach (var item in kasaItems.Where(k => !string.IsNullOrEmpty(k.DeviceIp)))
            {
#pragma warning disable CS8601 // Possible null reference argument.
                kasaSwitch.Address = item.DeviceIp;
#pragma warning restore CS8601 // Possible null reference argument.
                kasaSwitch.Outlet = item.Outlet;
                item.IsOn = kasaSwitch.IsOn;
            }
        }

        private void Matric_OnVariablesChanged(object sender, ServerVariablesChangedEventArgs data)
        {
            Console.WriteLine("Server variables changed");
            foreach (string varName in data.ChangedVariables)
            {
                Console.WriteLine($"{varName}: {data.Variables[varName].Value}");
                var currentItem = Config.KasaVariables.FirstOrDefault(v => v.Name?.Equals(varName) ?? false);
                if (currentItem == null) continue;

                currentItem.IsOn = (bool)(data.Variables[varName].Value);
            }
        }

        private void Matric_OnError(Exception ex) => MatricError = ex;

        private static void Matric_OnControlInteraction(object sender, object data)
        {
            Console.WriteLine("Control interaction:");
            Console.WriteLine(data.ToString());
        }

        private void Matric_OnConnectedClientsReceived(object source, List<ClientInfo> clients) => UpdateClientsList(clients);

        public void UpdateClientsList(List<ClientInfo> connectedClients)
        {
            if (connectedClients.Count == 0)
            {
                Console.WriteLine("No connected devices found, make sure your smartphone/tablet is connected\nPress any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
            Console.WriteLine("Found devices:");
            foreach (ClientInfo client in connectedClients)
            {
                Console.WriteLine($@"{client.Id} {client.Name}");
            }
            ClientId = connectedClients.FirstOrDefault()?.Id;
        }
    }
}
