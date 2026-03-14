using Microsoft.Extensions.Caching.Memory;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;

namespace ApiInator.Infrastructure;

public class HltbPythonIntegrator
{
    private readonly IPythonEnvironment _env;
    private readonly IMemoryCache _cache;

    public HltbPythonIntegrator(IPythonEnvironment env, IMemoryCache cache)
    {
        _env = env;
        _cache = cache;
    }

    public async Task<HltbResponse> FetchByNameAsync(string gameName)
    {
        string cacheKey = $"hltb_name_{gameName.ToLower()}";
        
        if (_cache.TryGetValue(cacheKey, out HltbResponse cachedResponse))
        {
            return cachedResponse;
        }

        var response = await Task.Run(() => 
        {
            var scrapper = _env.HltbScrapper();
            
            var pythonResult = scrapper.FetchGameByName(gameName); 
            return MapToResponse(pythonResult);
        });

        if (response.Status == "success")
        {
            _cache.Set(cacheKey, response, TimeSpan.FromHours(24));
        }

        return response;
    }

    public async Task<HltbResponse> FetchByIdAsync(int gameId)
    {
        string cacheKey = $"hltb_id_{gameId}";
        
        if (_cache.TryGetValue(cacheKey, out HltbResponse cachedResponse))
        {
            return cachedResponse;
        }

        var response = await Task.Run(() => 
        {
            var scrapper = _env.HltbScrapper();
            
            var pythonResult = scrapper.FetchGameById(gameId); 
            return MapToResponse(pythonResult);
        });

        if (response.Status == "success")
        {
            _cache.Set(cacheKey, response, TimeSpan.FromHours(24));
        }

        return response;
    }

    private HltbResponse MapToResponse(IReadOnlyDictionary<string, PyObject> result)
    {
        var response = new HltbResponse
        {
            Status = result.TryGetValue("status", out var s) ? s.As<string>() : "error",
        
            Message = result.TryGetValue("message", out var m) && m.ToString() != "None" ? m.As<string>() : null
        };

        if (response.Status == "success" && result.TryGetValue("data", out var rawData))
        {
            var data = rawData.As<IReadOnlyDictionary<string, PyObject>>();
        
            response.Data = new HLTBInfo
            {
                GameId = Convert.ToInt32(data["game_id"].As<long>()),
                GameName = data["game_name"].As<string>(),
                MainStory = data["main_story"].As<double>(),
                MainExtra = data["main_extra"].As<double>(),
                Completionist = data["completionist"].As<double>()
            };
        }

        return response;
    }
}

public class HltbResponse
{
    public string Status { get; set; }
    public HLTBInfo Data { get; set; }
    public string Message { get; set; }
}

public class HLTBInfo
{
    public int GameId { get; set; }
    public string GameName { get; set; }
    public double MainStory { get; set; }
    public double MainExtra { get; set; }
    public double Completionist { get; set; }
}