using ApiInator.Protos;
using SlimMessageBus;
using ApiInator.Application.SteamApi;
using Google.Protobuf.Collections;
using SearchGameRequest = ApiInator.Generated.SearchGameRequest;

namespace ApiInator.Generated
{
    public class SearchGameRequest : IRequest<SearchGameResponse>
    {
        public string Name { get; set; }
    }
}

namespace ApiInator.Application
{

    public class SearchGameHandler(SteamApi.SteamApi steamApi) : IRequestHandler<SearchGameRequest, SearchGameResponse>
    {
        public async Task<SearchGameResponse> OnHandle(SearchGameRequest request, CancellationToken cancellationToken)
        {
            var name = request.Name;
            try
            {
                var games = await steamApi.SearchByNameAsync(name);
                var response = new SearchGameResponse();
                response.Games.AddRange(games.Select(g => new GamePreview()
                        { Name = g.Name, SteamappId = g.SteamAppID.ToString(), TinyImage = g.TinyImage }));
                return response;
            }
            catch
            {
                return new SearchGameResponse();
            }
        }
    }
}