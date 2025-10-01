using System.Net;
using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Exceptions;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Constructor_WithDefaultMessage_ShouldSetDefaultMessage()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.Message.Should().Be("Unauthorized access.");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetCustomMessage()
    {
        // Arrange
        string customMessage = "User authentication required";

        // Act
        var exception = new UnauthorizedException(customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.ErrorCode.Should().Be("unauthorized");
    }

    [Fact]
    public void Constructor_ShouldSetStatusCode()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetErrorCode()
    {
        // Arrange
        string customMessage = "Token expired";

        // Act
        var exception = new UnauthorizedException(customMessage);

        // Assert
        exception.ErrorCode.Should().Be("unauthorized");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetStatusCode()
    {
        // Arrange
        string customMessage = "Token expired";

        // Act
        var exception = new UnauthorizedException(customMessage);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
