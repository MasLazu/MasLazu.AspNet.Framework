namespace MasLazu.AspNet.Framework.Endpoint.Models;

/// <summary>
/// Standard error response model for API errors
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; } = 500;

    /// <summary>
    /// Error code identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (can be validation errors, exception details, etc.)
    /// </summary>
    public object? Errors { get; set; }
}

/// <summary>
/// Validation error response model
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    public ValidationErrorResponse()
    {
        StatusCode = 400;
        Code = "validation_error";
        Message = "Validation failed";
    }

    /// <summary>
    /// Validation errors organized by field name
    /// </summary>
    public new Dictionary<string, string[]>? Errors { get; set; }
}
