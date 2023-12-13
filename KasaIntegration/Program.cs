using KasaMatricIntegration.MatricIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MatricService>(); 
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Kasa Matric Integration";
});

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

#if(DEBUG)
    logging.AddConsole();
    logging.AddDebug();
#endif
});
using var host = builder.Build();

MatricPrompt.MatricPin(builder.Configuration);

await host.RunAsync();