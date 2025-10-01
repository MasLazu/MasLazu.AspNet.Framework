using System.Net;
using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Exceptions;

public class ForbiddenExceptionTests
{
    [Fact]
    public void Constructor_WithDefaultMessage_ShouldSetDefaultMessage()
    {
        // Act
        var exception = new ForbiddenException();

        // Assert
        exception.Message.Should().Be("Access forbidden.");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetCustomMessage()
    {
        // Arrange
        string customMessage = "Insufficient permissions";

        // Act
        var exception = new ForbiddenException(customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Act
        var exception = new ForbiddenException();

        // Assert
        exception.ErrorCode.Should().Be("forbidden");
    }

    [Fact]
    public void Constructor_ShouldSetStatusCode()
    {
        // Act
        var exception = new ForbiddenException();

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetErrorCode()
    {
        // Arrange
        string customMessage = "Admin access required";

        // Act
        var exception = new ForbiddenException(customMessage);

        // Assert
        exception.ErrorCode.Should().Be("forbidden");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetStatusCode()
    {
        // Arrange
        string customMessage = "Admin access required";

        // Act
        var exception = new ForbiddenException(customMessage);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
