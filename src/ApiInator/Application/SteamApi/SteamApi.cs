using System.Net.Http.Json; // Kluczowe do GetFromJsonAsync
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ApiInator.Application.SteamApi;

public class SteamResponse
{
    [JsonPropertyName("items")]
    public List<SteamItem> Items { get; set; } = new();
}

public class SteamItem
{
    [JsonPropertyName("id")]
    public int SteamAppID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("metascore")]
    public string Metascore { get; set; } = string.Empty;

    [JsonPropertyName("tiny_image")]
    public string TinyImage { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public PriceDetails? Price { get; set; }

    [JsonIgnore]
    public int InitialPrice => Price?.InitialPrice ?? 0;

    [JsonIgnore]
    public string Currency => Price?.Currency ?? "PLN";
}

public class PriceDetails
{
    [JsonPropertyName("initial")]
    public int InitialPrice { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}

public class SteamApi
{
    private readonly ILogger<SteamApi> _logger;
    private readonly HttpClient _steamClient;
    private const string BASE_URL = "https://store.steampowered.com/api/";

    public SteamApi(ILogger<SteamApi> logger)
    {
        _logger = logger;
        _steamClient = new HttpClient();
    }

    public async Task<IReadOnlyList<SteamItem>> SearchByNameAsync(string name)
    {
        try
        {
            var url = $"{BASE_URL}storesearch/?term={Uri.EscapeDataString(name)}&l=english&cc=PL";
            var steamData = await _steamClient.GetFromJsonAsync<SteamResponse>(url);
            
            return steamData?.Items ?? (IReadOnlyList<SteamItem>)Array.Empty<SteamItem>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Communication error for: {Name}", name);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Deserialization error for: {Name}", name);
        }

        return Array.Empty<SteamItem>();
    }
}