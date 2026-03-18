namespace ApiInator.Application.GGDealsApi;

public static class GgDealsConfiguration
{
    public static void AddGgDealsApi(this IServiceCollection services)
    {
        services.AddHttpClient<GgDealsApiClient>();
    }
}