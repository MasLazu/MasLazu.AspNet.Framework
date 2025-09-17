using System.Net;

namespace MasLazu.AspNet.Framework.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string resourceName, object resourceId)
        : base($"The {resourceName} with id '{resourceId}' was not found.",
              "not_found",
              HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string message)
        : base(message, "not_found", HttpStatusCode.NotFound)
    {
    }
}
