using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ApiInator.Application.SteamApi;

public class SteamSearchResponse
{
    [JsonPropertyName("items")]
    public List<SteamSearchItem> Items { get; set; } = new();
}

public class SteamSearchItem
{
    [JsonPropertyName("id")]
    public int SteamAppID { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("tiny_image")]
    public string TinyImage { get; set; } = string.Empty;

}

public class SteamDetailsItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("short_description")]
    public string ShortDescription { get; set; } = string.Empty;

    [JsonPropertyName("genres")]
    public List<Genre> Genres { get; set; } = new();

    [JsonPropertyName("platforms")]
    public Platforms Platforms { get; set; }

    [JsonPropertyName("price_overview")]
    public DetailsPrice? Price { get; set; }

    [JsonPropertyName("developers")]
    public List<string> Developers { get; set; } = new();

    [JsonPropertyName("release_date")] 
    public ReleaseDate ReleaseDate { get; set; }
}

public record Genre(string description);
public record Platforms(bool windows, bool mac, bool linux);
public record DetailsPrice(int initial, int final);
public record ReleaseDate(string date);

public class SteamApi
{
    private readonly ILogger<SteamApi> _logger;
    private readonly HttpClient _steamClient;
    private const string BASE_URL = "https://store.steampowered.com/api";

    public SteamApi(ILogger<SteamApi> logger)
    {
        _logger = logger;
        _steamClient = new HttpClient();
    }

    public async Task<SteamDetailsItem?> GetGameDetailsAsync(string steamId)
    {
        try
        {
            var url = $"{BASE_URL}/appdetails?appids={steamId}&cc=PL&l=polish";
            var response = await _steamClient.GetFromJsonAsync<Dictionary<string, JsonElement>>(url);

            if (response != null && response.TryGetValue(steamId, out var root))
            {
                if (root.GetProperty("success").GetBoolean())
                {
                    return JsonSerializer.Deserialize<SteamDetailsItem>(root.GetProperty("data").GetRawText());
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Communication error for: {Id}", steamId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Deserialization error for: {Id}", steamId);
        }

        return null;
    }

    public async Task<IReadOnlyList<SteamSearchItem>> SearchPreviewByNameAsync(string name)
    {
        try
        {
            var url = $"{BASE_URL}/storesearch/?term={Uri.EscapeDataString(name)}&l=polish&cc=PL";
            var steamData = await _steamClient.GetFromJsonAsync<SteamSearchResponse>(url);
            
            return steamData?.Items ?? new List<SteamSearchItem>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Communication error for: {Name}", name);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Deserialization error for: {Name}", name);
        }

        return new List<SteamSearchItem>();
    }
}