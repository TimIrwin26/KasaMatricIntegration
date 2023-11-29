using Microsoft.Extensions.Configuration;

namespace KasaIntegration
{
    public static class MatricPrompt
    {
        public static string MatricPin(IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration["Matric:Pin"])) return configuration["Matric:Pin"] ?? "";

            Console.WriteLine("Authorize connection in MATRIC, then enter PIN:");
            var matric = new Matric.Integration.Matric(MatricService.ApplicationName, Convert.ToInt32(configuration["Matric:ApiPort"]));
            matric.RequestAuthorizePrompt();
            configuration["Matric:Pin"] = Console.ReadLine();

            return configuration["Matric:Pin"] ?? "";
        }
    }
}
