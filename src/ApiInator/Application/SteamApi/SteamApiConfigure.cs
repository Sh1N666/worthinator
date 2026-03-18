namespace ApiInator.Application.SteamApi;

public static class SteamApiConfigure
{
    public static void AddSteamApi(this IServiceCollection services)
    {
        services.AddHttpClient<SteamApi>();
    }
}