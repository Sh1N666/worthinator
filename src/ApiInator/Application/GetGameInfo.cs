using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SlimMessageBus;
using ApiInator.Generated;
using ApiInator.Application.GGDealsApi;
using ApiInator.Model;
using GgDealsData = ApiInator.Generated.GgDealsData;
using GgDealsPricesData = ApiInator.Generated.GgDealsPricesData;
using HltbData = ApiInator.Generated.HltbData;
using MetacriticData = ApiInator.Generated.MetacriticData;
using SteamPriceData = ApiInator.Generated.SteamPriceData;

namespace ApiInator.Generated
{
    public partial class GetGameInfoRequest : IRequest<GetGameInfoResponse>
    {
    }
}

namespace ApiInator.Application
{
    public class GetGameInfoHandler(
        ApplicationContext dbContext,
        SteamApi.SteamApi steamApi,
        HowLongToBeatApi.HowLongToBeatApi hltbApi,
        GgDealsApiClient ggDealsApi,
        ILogger<GetGameInfoHandler> logger) : IRequestHandler<GetGameInfoRequest, GetGameInfoResponse>
    {
        public async Task<GetGameInfoResponse> OnHandle(GetGameInfoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Rozpoczęto przetwarzanie żądania dla SteamAppId: {AppId}", request?.SteamAppId);
                
                if (request == null)
                {
                    logger.LogWarning("Och, Panie, przesłane żądanie jest puste (null). Przerywam działanie.");
                    return new GetGameInfoResponse();
                }

                var appId = request.SteamAppId;

                logger.LogInformation("Szukam gry o SteamAppId {AppId} w Twojej wspaniałej bazie danych.", appId);
                var cachedGame = await dbContext.Games
                    .Find(g => g.SteamAppId == appId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (cachedGame != null)
                {
                    logger.LogInformation("Gra o SteamAppId {AppId} znaleziona w bazie. Zwracam wynik bez niepotrzebnego fatygowania zewnętrznych API.", appId);
                    return MapToResponse(cachedGame);
                }

                logger.LogInformation("Gry o SteamAppId {AppId} nie ma w bazie. Pytam Steam o detale...", appId);
                var steamDetails = await steamApi.GetGameDetailsAsync(appId.ToString());
                if (steamDetails == null)
                {
                    logger.LogWarning("Zuchwały Steam nie zwrócił żadnych detali dla SteamAppId {AppId}.", appId);
                    return new GetGameInfoResponse();
                }

                logger.LogInformation("Pobrano detale ze Steam dla gry {AppId} ({Name}). Odpytuję równolegle HLTB oraz GGDeals...", appId, steamDetails.Name);
                var hltbTask = hltbApi.GetByNameAsync(steamDetails.Name);
                var ggDealsTask = ggDealsApi.GetGamePricesAsync(appId.ToString());

                await Task.WhenAll(hltbTask, ggDealsTask);
                logger.LogInformation("Zakończono pobieranie danych z zewnętrznych usług dla gry {AppId}.", appId);

                var hltbData = await hltbTask;
                var ggDealsData = await ggDealsTask;

                var newGameDocument = new GameData
                {
                    SteamAppId = appId,
                    Name = steamDetails.Name,
                    ShortDescription = steamDetails.ShortDescription,
                    ReleaseDate = steamDetails.ReleaseDate.date,
                    Developers = steamDetails.Developers,
                    Genres = steamDetails.Genres?.Select(g => g.description).ToList() ?? new(),
                    
                    Platforms = steamDetails.Platforms != null ? new GamePlatforms
                    {
                        Windows = steamDetails.Platforms.windows,
                        Mac = steamDetails.Platforms.mac,
                        Linux = steamDetails.Platforms.linux
                    } : null,
                    
                    Metacritic = steamDetails.Metacritic != null ? new Model.MetacriticData
                    {
                        Score = steamDetails.Metacritic.score,
                        Url = steamDetails.Metacritic.url
                    } : null,
                    
                    SteamPrice = steamDetails.Price != null ? new Model.SteamPriceData
                    {
                        Initial = steamDetails.Price.initial,
                        Final = steamDetails.Price.final
                    } : null,
                    
                    Hltb = hltbData != null ? new Model.HltbData
                    {
                        GameId = hltbData.Data.GameId,
                        MainStory = hltbData.Data.MainStory,
                        MainExtra = hltbData.Data.MainExtra,
                        Completionist = hltbData.Data.Completionist,
                        ReviewScore = hltbData.Data.ReviewScore
                    } : null,
                    
                    GgDeals = ggDealsData != null ? new Model.GgDealsData
                    {
                        Url = ggDealsData.Url,
                        Prices = ggDealsData.Prices != null ? new Model.GgDealsPricesData
                        {
                            CurrentRetail = ggDealsData.Prices.CurrentRetail,
                            CurrentKeyshops = ggDealsData.Prices.CurrentKeyshops,
                            HistoricalRetail = ggDealsData.Prices.HistoricalRetail,
                            HistoricalKeyshops = ggDealsData.Prices.HistoricalKeyshops,
                            Currency = ggDealsData.Prices.Currency
                        } : null
                    } : null
                };

                logger.LogInformation("Zapisuję zebrane dane dla gry {AppId} do bazy...", appId);
                var filter = Builders<GameData>.Filter.Eq(g => g.SteamAppId, appId);
                var options = new FindOneAndReplaceOptions<GameData> 
                { 
                    IsUpsert = true, 
                    ReturnDocument = ReturnDocument.After 
                };
                
                var savedGame = await dbContext.Games.FindOneAndReplaceAsync(filter, newGameDocument, options, cancellationToken);
                logger.LogInformation("Dane dla gry {AppId} zostały pomyślnie utrwalone w Twojej domenie.", appId);

                return MapToResponse(savedGame ?? newGameDocument);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Wystąpił potworny błąd podczas przetwarzania żądania dla SteamAppId: {AppId}", request?.SteamAppId);
                return new GetGameInfoResponse();
            }
        }

        private GetGameInfoResponse MapToResponse(GameData game)
        {
            var response = new GetGameInfoResponse
            {
                Id = game.Id,
                SteamAppId = game.SteamAppId,
                Name = game.Name,
                ShortDescription = game.ShortDescription,
                ReleaseDate = game.ReleaseDate,
                WorthFactor = WorthCalculator.CalculateWorthFactor(game)
            };

            if (game.Developers != null && response.Developers != null) response.Developers.AddRange(game.Developers);
            if (game.Genres != null && response.Genres != null) response.Genres.AddRange(game.Genres);

            if (game.Platforms != null)
            {
                response.Platforms = new Platforms
                {
                    Windows = game.Platforms.Windows,
                    Mac = game.Platforms.Mac,
                    Linux = game.Platforms.Linux
                };
            }

            if (game.Metacritic != null)
            {
                response.Metacritic = new MetacriticData
                {
                    Score = game.Metacritic.Score,
                    Url = game.Metacritic.Url
                };
            }

            if (game.SteamPrice != null)
            {
                response.SteamPrice = new SteamPriceData
                {
                    Initial = game.SteamPrice.Initial,
                    Final = game.SteamPrice.Final
                };
            }

            if (game.Hltb != null)
            {
                response.Hltb = new HltbData
                {
                    GameId = game.Hltb.GameId,
                    MainStory = game.Hltb.MainStory,
                    MainExtra = game.Hltb.MainExtra,
                    Completionist = game.Hltb.Completionist,
                    ReviewScore = game.Hltb.ReviewScore
                };
            }

            if (game.GgDeals != null)
            {
                response.GgDeals = new GgDealsData
                {
                    Url = game.GgDeals.Url ?? string.Empty
                };

                if (game.GgDeals.Prices != null)
                {
                    response.GgDeals.Prices = new GgDealsPricesData
                    {
                        CurrentRetail = game.GgDeals.Prices.CurrentRetail,
                        CurrentKeyshops = game.GgDeals.Prices.CurrentKeyshops,
                        HistoricalRetail = game.GgDeals.Prices.HistoricalRetail,
                        HistoricalKeyshops = game.GgDeals.Prices.HistoricalKeyshops,
                        Currency = game.GgDeals.Prices.Currency ?? string.Empty
                    };
                }
            }

            return response;
        }
    }
}