using System.Net;
using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Exceptions;

public class ConflictExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Resource already exists";

        // Act
        var exception = new ConflictException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Arrange
        string message = "Duplicate entry";

        // Act
        var exception = new ConflictException(message);

        // Assert
        exception.ErrorCode.Should().Be("conflict");
    }

    [Fact]
    public void Constructor_ShouldSetStatusCode()
    {
        // Arrange
        string message = "Resource conflict";

        // Act
        var exception = new ConflictException(message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("Email already in use")]
    [InlineData("Username is taken")]
    [InlineData("Resource version mismatch")]
    public void Constructor_WithVariousMessages_ShouldSetMessageCorrectly(string message)
    {
        // Act
        var exception = new ConflictException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("conflict");
        exception.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
