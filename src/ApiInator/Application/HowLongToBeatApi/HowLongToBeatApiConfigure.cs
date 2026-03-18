using ApiInator.Infrastructure;
using CSnakes.Runtime;


namespace ApiInator.Application.HowLongToBeatApi;

public static class HowLongToBeatApiConfigure
{
    public static void AddHLTBApi(this IServiceCollection services)
    { 
        string infraPath = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure");
        string venvPath = Path.Combine(infraPath, "venv");

        services.WithPython()
            .WithHome(infraPath)
            .WithVirtualEnvironment(venvPath)
            .FromRedistributable();

        services.AddSingleton<HltbPythonIntegrator>();
        services.AddScoped<HowLongToBeatApi>();
        services.AddMemoryCache();
    }
}