using KasaMatricIntegration.MatricIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<MatricService>();
builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddConsole();
    logging.AddDebug();
});
using var host = builder.Build();

MatricPrompt.MatricPin(builder.Configuration);

await host.RunAsync();