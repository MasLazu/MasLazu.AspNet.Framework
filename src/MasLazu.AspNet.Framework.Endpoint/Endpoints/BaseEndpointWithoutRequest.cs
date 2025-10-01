using FastEndpoints;
using Microsoft.AspNetCore.Identity.Data;
using MasLazu.AspNet.Framework.Endpoint.EndpointGroups;
using MasLazu.AspNet.Framework.Endpoint.Models;
using System.Security.Claims;
using MasLazu.AspNet.Framework.Application.Exceptions;

namespace MasLazu.AspNet.Framework.Endpoint.Endpoints;

public abstract class BaseEndpointWithoutRequest<TResponse> :
    EndpointWithoutRequest<SuccessResponse<TResponse>>
    where TResponse : notnull
{
    public override void Configure()
    {
        ConfigureEndpoint();
        DontCatchExceptions();
        // Idempotency();
        // Group<V1EndpointGroup>();
        // EnableAntiforgery();
    }

    public Guid GetUserId()
    {
        Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            throw new UnauthorizedException("User is not authenticated");
        }

        return userId;
    }

    public abstract void ConfigureEndpoint();

    public async Task SendSuccessResponseAsync(TResponse data, int statusCode = 200, string? message = null)
    {
        await Send.OkAsync(SuccessResponse<TResponse>.CreateSuccess(data, message, statusCode));
    }

    public async Task SendOkResponseAsync(TResponse data, string? message = null)
    {
        await Send.OkAsync(SuccessResponse<TResponse>.CreateSuccess(data, message, 200));
    }
}