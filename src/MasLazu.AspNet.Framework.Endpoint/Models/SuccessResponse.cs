using MasLazu.AspNet.Framework.Application.Models;

namespace MasLazu.AspNet.Framework.Endpoint.Models;

/// <summary>
/// Generic success response model for API responses
/// </summary>
/// <typeparam name="T">The type of the response data</typeparam>
public class SuccessResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = "Operation completed successfully";

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Creates a success response with data
    /// </summary>
    public static SuccessResponse<T> CreateSuccess(T data, string? message = null, int statusCode = 200)
    {
        return new SuccessResponse<T>
        {
            Data = data,
            Message = message ?? "Operation completed successfully",
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates a success response without data
    /// </summary>
    public static SuccessResponse<T> CreateSuccess(string? message = null, int statusCode = 200)
    {
        return new SuccessResponse<T>
        {
            Message = message ?? "Operation completed successfully",
            StatusCode = statusCode
        };
    }
}
