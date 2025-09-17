using System.Net;

namespace MasLazu.AspNet.Framework.Application.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource)
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, "conflict", HttpStatusCode.Conflict)
    {
    }
}
