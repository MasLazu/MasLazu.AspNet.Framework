using Microsoft.AspNetCore.Builder;
using MasLazu.AspNet.Framework.Endpoint.Middlewares;

namespace MasLazu.AspNet.Framework.Endpoint.Extensions;

/// <summary>
/// Extension methods for registering the exception handler middleware
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseFrameworkExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
