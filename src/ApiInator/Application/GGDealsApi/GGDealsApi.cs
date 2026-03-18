using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ApiInator.Application.GGDealsApi;

public class GgDealsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, GgDealsGameData?> Data { get; set; } = new();
}

public class GgDealsGameData
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("prices")]
    public GgDealsPrices? Prices { get; set; }
}

public class GgDealsPrices
{
    [JsonPropertyName("currentRetail")]
    public double CurrentRetail { get; set; }

    [JsonPropertyName("currentKeyshops")]
    public double CurrentKeyshops { get; set; }

    [JsonPropertyName("historicalRetail")]
    public double HistoricalRetail { get; set; }

    [JsonPropertyName("historicalKeyshops")]
    public double HistoricalKeyshops { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}

public class GgDealsApiClient
{
    private readonly ILogger<GgDealsApiClient> _logger;
    private readonly HttpClient _httpClient;
    private static readonly string API_KEY = Environment.GetEnvironmentVariable("GGDEALS_API_KEY") ?? string.Empty;
    private static readonly string BASE_URL = "https://api.gg.deals/v1/prices";

    public GgDealsApiClient(ILogger<GgDealsApiClient> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GgDealsGameData?> GetGamePricesAsync(string steamId)
    {
        try
        {
            var url = $"{BASE_URL}/by-steam-app-id/?ids={steamId}&key={API_KEY}&region=pl";
            var response = await _httpClient.GetFromJsonAsync<GgDealsResponse>(url);

            if (response != null && response.Success && response.Data.TryGetValue(steamId, out var gameData))
            {
                return gameData;
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
}