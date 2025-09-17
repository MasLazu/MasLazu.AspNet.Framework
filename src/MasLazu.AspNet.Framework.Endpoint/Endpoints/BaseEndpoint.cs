using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Identity.Data;
using MasLazu.AspNet.Framework.Endpoint.EndpointGroups;
using MasLazu.AspNet.Framework.Endpoint.Models;

namespace MasLazu.AspNet.Framework.Endpoint.Endpoints;

public abstract class BaseEndpoint<TRequest, TResponse> :
    Endpoint<TRequest, SuccessResponse<TResponse>>
    where TRequest : notnull
{
    public override void Configure()
    {
        ConfigureEndpoint();
        DontCatchExceptions();
        // Idempotency();
        // EnableAntiforgery();
        // Group<V1EndpointGroup>();
    }

    public abstract void ConfigureEndpoint();

    public async Task SendSuccessResponseAsync(TResponse data, int statusCode = 200, string? message = null, CancellationToken ct = default)
    {
        await Send.OkAsync(SuccessResponse<TResponse>.CreateSuccess(data, message, statusCode), ct);
    }

    public async Task SendOkResponseAsync(TResponse data, string? message = null, CancellationToken ct = default)
    {
        await Send.OkAsync(SuccessResponse<TResponse>.CreateSuccess(data, message, 200), ct);
    }
}