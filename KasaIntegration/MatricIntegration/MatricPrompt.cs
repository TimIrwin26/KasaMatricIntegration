using Microsoft.Extensions.Configuration;

namespace KasaMatricIntegration.MatricIntegration
{
    public static class MatricPrompt
    {
        public static string MatricPin(IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration["Matric:Pin"])) return configuration["Matric:Pin"] ?? "";

            Console.WriteLine("Authorize connection in MATRIC, then enter PIN:");
            var matric = new global::Matric.Integration.Matric(configuration["Matric:ApplicationName"], Convert.ToInt32(configuration["Matric:ApiPort"]));
            matric.RequestAuthorizePrompt();
            configuration["Matric:Pin"] = Console.ReadLine();

            return configuration["Matric:Pin"] ?? "";
        }
    }
}
