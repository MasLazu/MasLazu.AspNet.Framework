using System.Net;

namespace MasLazu.AspNet.Framework.Application.Exceptions;

/// <summary>
/// Exception thrown when the user is not authorized to perform an action
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message, "unauthorized", HttpStatusCode.Unauthorized)
    {
    }
}
