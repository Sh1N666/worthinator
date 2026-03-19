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
                logger.LogInformation("Start searching for SteamAppId: {AppId}", request?.SteamAppId);
                
                if (request == null)
                {
                    logger.LogWarning("Request is (null).");
                    return new GetGameInfoResponse();
                }

                var appId = request.SteamAppId;

                logger.LogInformation("Searching for {AppId} in cached data.", appId);
                var cachedGame = await dbContext.Games
                    .Find(g => g.SteamAppId == appId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (cachedGame != null)
                {
                    logger.LogInformation("Game with SteamAppId {AppId} has been found", appId);
                    if (cachedGame.LastUpdated - DateTimeOffset.Now > TimeSpan.FromHours(24))
                    {
                        var ggDealsPrices = await ggDealsApi.GetGamePricesAsync(appId.ToString());

                        cachedGame.LastUpdated = DateTimeOffset.Now;
                        cachedGame.GgDeals.Prices.CurrentKeyshops = ggDealsPrices.Prices.CurrentKeyshops;
                        cachedGame.GgDeals.Prices.CurrentRetail = ggDealsPrices.Prices.CurrentRetail;
                        cachedGame.GgDeals.Prices.HistoricalKeyshops = cachedGame.GgDeals.Prices.HistoricalKeyshops;
                        cachedGame.GgDeals.Prices.HistoricalRetail = cachedGame.GgDeals.Prices.HistoricalRetail;
                        var saveFilter = Builders<GameData>.Filter.Eq(g => g.SteamAppId, appId);
                        var saveOptions = new FindOneAndReplaceOptions<GameData> 
                        { 
                            IsUpsert = true
                        };
                        await dbContext.Games.FindOneAndReplaceAsync(saveFilter, cachedGame, saveOptions, cancellationToken);
                    }
                    return MapToResponse(cachedGame);
                }

                logger.LogInformation("Game with SteamAppId {AppId} is absent in cached data.", appId);
                var steamDetails = await steamApi.GetGameDetailsAsync(appId.ToString());
                if (steamDetails == null)
                {
                    logger.LogWarning("Steam don't steaming for {AppId}.", appId);
                    return new GetGameInfoResponse();
                }

                var ggDealsTask = ggDealsApi.GetGamePricesAsync(appId.ToString());
                var ggDealsData = await ggDealsTask;
                var cleanName = new string(ggDealsData.Title.Where(c => !char.IsSymbol(c)).ToArray());
                
                var hltbTask = hltbApi.GetByNameAsync(cleanName);

                var hltbData = await hltbTask;
                
                await Task.WhenAll(hltbTask, ggDealsTask);
                logger.LogInformation("Get details for {AppId} ({Name}) successfully.", appId, steamDetails.Name);
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

                logger.LogInformation("Saving game  {AppId} into cached data", appId);
                var filter = Builders<GameData>.Filter.Eq(g => g.SteamAppId, appId);
                var options = new FindOneAndReplaceOptions<GameData> 
                { 
                    IsUpsert = true, 
                    ReturnDocument = ReturnDocument.After 
                };
                
                var savedGame = await dbContext.Games.FindOneAndReplaceAsync(filter, newGameDocument, options, cancellationToken);
                logger.LogInformation("Data for {AppId} saved successfully", appId);

                return MapToResponse(savedGame ?? newGameDocument);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occured while attempting to save {AppId}", request?.SteamAppId);
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
                    Url = game.GgDeals.Url
                };

                if (game.GgDeals.Prices != null)
                {
                    response.GgDeals.Prices = new GgDealsPricesData
                    {
                        CurrentRetail = game.GgDeals.Prices.CurrentRetail,
                        CurrentKeyshops = game.GgDeals.Prices.CurrentKeyshops,
                        HistoricalRetail = game.GgDeals.Prices.HistoricalRetail,
                        HistoricalKeyshops = game.GgDeals.Prices.HistoricalKeyshops,
                        Currency = game.GgDeals.Prices.Currency
                    };
                }
            }

            return response;
        }
    }
}