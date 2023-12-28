using KasaMatricIntegration.Kasa;
using KasaMatricIntegration.MatricIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PyKasa.Net;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IKasaDeviceFactory>((service) =>
{
    var configuration = service.GetService<IConfiguration>()?.GetSection("Python");
    var dll = configuration?["PythonDll"] ?? string.Empty;
    var timeout = configuration?.GetValue<int?>("timeout") ?? 20;
    return new KasaDeviceFactory(dll, timeout);
});

builder.Services.AddSingleton<IKasaDeviceService, KasaDeviceService>();

builder.Services.AddHostedService<MatricService>();
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Kasa Matric Integration";
});

builder.Services.AddLogging(logging =>
{
    var loggingSection = builder.Configuration.GetSection("Logging");
    logging.AddConfiguration(loggingSection);
    logging.AddFile(loggingSection);
#if DEBUG
    logging.AddConsole();
    logging.AddDebug();
#endif
});
using var host = builder.Build();

MatricPrompt.MatricPin(builder.Configuration);

await host.RunAsync();