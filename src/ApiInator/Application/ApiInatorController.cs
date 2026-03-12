using ApiInator.Protos;
using Grpc.Core;
using SlimMessageBus;
using SlimMessageBus.Host;

namespace ApiInator.Application;

public class ApiInatorController(IMessageBus messageBus) : WorthinatorService.WorthinatorServiceBase
{
    public override async Task<SearchGameResponse> SearchGame(SearchGameRequest request, ServerCallContext context)
    {
        var searchRequest = new Generated.SearchGameRequest()
        {
            Name = request.SearchPhrase
        };
        var response = await messageBus.Send(searchRequest);
        
        return response;
    }
}