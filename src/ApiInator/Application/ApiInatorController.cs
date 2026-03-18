using ApiInator.Generated;
using Grpc.Core;
using SlimMessageBus;
using SlimMessageBus.Host;

namespace ApiInator.Application;

public class ApiInatorController(IMessageBus messageBus) : WorthinatorService.WorthinatorServiceBase
{
    public override async Task<SearchGameResponse> SearchGame(SearchGameRequest request, ServerCallContext context)
    {
        var response = await messageBus.Send(request);
        return response;
    }

    public override async Task<GetGameInfoResponse> GetGameInfo(GetGameInfoRequest request, ServerCallContext context)
    {
        var response = await messageBus.Send(request);
        return response;
    }
}