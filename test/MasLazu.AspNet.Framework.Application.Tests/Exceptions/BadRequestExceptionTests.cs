using System.Net;
using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Exceptions;

public class BadRequestExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        string message = "Invalid request data";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetErrorCode()
    {
        // Arrange
        string message = "Invalid request data";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        exception.ErrorCode.Should().Be("bad_request");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetStatusCode()
    {
        // Arrange
        string message = "Invalid request data";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetDetails()
    {
        // Arrange
        string message = "Invalid request data";
        var details = new { Field = "Email", Error = "Invalid format" };

        // Act
        var exception = new BadRequestException(message, details);

        // Assert
        exception.Details.Should().Be(details);
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetMessage()
    {
        // Arrange
        string message = "Invalid request data";
        var details = new { Field = "Email" };

        // Act
        var exception = new BadRequestException(message, details);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetErrorCode()
    {
        // Arrange
        string message = "Invalid request data";
        var details = new { Field = "Email" };

        // Act
        var exception = new BadRequestException(message, details);

        // Assert
        exception.ErrorCode.Should().Be("bad_request");
    }

    [Fact]
    public void Constructor_WithMessageAndDetails_ShouldSetStatusCode()
    {
        // Arrange
        string message = "Invalid request data";
        var details = new { Field = "Email" };

        // Act
        var exception = new BadRequestException(message, details);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void Constructor_WithMessageOnly_ShouldHaveNullDetails()
    {
        // Arrange
        string message = "Invalid request data";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        exception.Details.Should().BeNull();
    }
}
