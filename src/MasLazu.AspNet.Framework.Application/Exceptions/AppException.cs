using System.Net;

namespace MasLazu.AspNet.Framework.Application.Exceptions;

/// <summary>
/// Base exception for application-level errors that can be handled to return responses to the client
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// HTTP status code to return to the client
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Application-specific error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Additional details about the error
    /// </summary>
    public object? Details { get; }

    protected AppException(
        string message,
        string errorCode,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        object? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }
}
