using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using MasLazu.AspNet.Framework.Application.Exceptions;
using MasLazu.AspNet.Framework.Endpoint.Models;
using FastEndpoints;
using FluentValidation.Results;

namespace MasLazu.AspNet.Framework.Endpoint.Middlewares;

/// <summary>
/// Middleware for handling application exceptions and returning appropriate HTTP responses
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await HandleAppExceptionAsync(context, ex);
        }
        catch (ValidationFailureException ex)
        {
            await HandleValidationFailureExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    private async Task HandleAppExceptionAsync(HttpContext context, AppException ex)
    {
        _logger.LogWarning(ex, "Application exception occurred: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);

        var response = new Models.ErrorResponse
        {
            StatusCode = (int)ex.StatusCode,
            Code = ex.ErrorCode,
            Message = ex.Message,
            Errors = ex.Details
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        }));
    }

    private async Task HandleValidationFailureExceptionAsync(HttpContext context, ValidationFailureException ex)
    {
        _logger.LogWarning("Validation failed: {Failures}", ex.Failures);

        var errors = (ex.Failures ?? new List<ValidationFailure>())
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray()
            );

        var response = new ValidationErrorResponse
        {
            Errors = errors
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        }));
    }

    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unexpected exception occurred: {Message}", ex.Message);

        var response = new Models.ErrorResponse
        {
            StatusCode = 500,
            Code = "internal_server_error",
            Message = "An unexpected error occurred. Please try again later.",
            Errors = (object?)null
#if DEBUG
                ?? new
                {
                    exception = ex.GetType().Name,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                }
#endif
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        }));
    }
}
