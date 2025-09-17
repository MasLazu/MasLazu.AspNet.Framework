using System.Net;

namespace MasLazu.AspNet.Framework.Application.Exceptions;

/// <summary>
/// Exception thrown when a request is invalid or contains invalid data
/// </summary>
public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, "bad_request", HttpStatusCode.BadRequest)
    {
    }

    public BadRequestException(string message, object? details)
        : base(message, "bad_request", HttpStatusCode.BadRequest, details)
    {
    }
}
