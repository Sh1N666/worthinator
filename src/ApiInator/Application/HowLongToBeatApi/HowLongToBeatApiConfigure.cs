using ApiInator.Infrastructure;
using CSnakes.Runtime;

namespace ApiInator.Application.HowLongToBeatApi;

public static class HowLongToBeatApiConfigure
{
    public static void AddHLTBApi(this IServiceCollection services)
    { 
        string venvPath = Path.Combine(Directory.GetCurrentDirectory(), "ApiInator.Infrastructure", ".venv");
        
        services.WithPython()
            .WithVirtualEnvironment(venvPath)
            .WithPipInstaller(); 

        services.AddSingleton<HltbPythonIntegrator>();
        services.AddScoped<HowLongToBeatApi>();
        services.AddMemoryCache();
    }
}