using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Identity.Data;
using MasLazu.AspNet.Framework.Endpoint.EndpointGroups;
using MasLazu.AspNet.Framework.Endpoint.Models;

namespace MasLazu.AspNet.Framework.Endpoint.Endpoints;

public abstract class BaseEndpointWithoutResponse<TRequest> :
    BaseEndpoint<TRequest, object?>
    where TRequest : notnull
{
    public async Task SendSuccessResponseAsync(int statusCode = 200, string? message = null, CancellationToken ct = default)
    {
        await Send.OkAsync(SuccessResponse<object?>.CreateSuccess(null, message, statusCode), ct);
    }

    public async Task SendOkResponseAsync(string? message = null, CancellationToken ct = default)
    {
        await Send.OkAsync(SuccessResponse<object?>.CreateSuccess(null, message, 200), ct);
    }
}