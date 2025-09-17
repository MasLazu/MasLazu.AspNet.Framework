using System.Net;

namespace MasLazu.AspNet.Framework.Application.Exceptions;

/// <summary>
/// Exception thrown when the user is authenticated but not allowed to perform an action
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Access forbidden.")
        : base(message, "forbidden", HttpStatusCode.Forbidden)
    {
    }
}
