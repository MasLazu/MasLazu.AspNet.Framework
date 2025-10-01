using System.Net;
using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithResourceNameAndId_ShouldSetMessage()
    {
        // Arrange
        string resourceName = "Product";
        var resourceId = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(resourceName, resourceId);

        // Assert
        exception.Message.Should().Be($"The {resourceName} with id '{resourceId}' was not found.");
    }

    [Fact]
    public void Constructor_WithResourceNameAndId_ShouldSetErrorCode()
    {
        // Arrange
        string resourceName = "Product";
        var resourceId = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(resourceName, resourceId);

        // Assert
        exception.ErrorCode.Should().Be("not_found");
    }

    [Fact]
    public void Constructor_WithResourceNameAndId_ShouldSetStatusCode()
    {
        // Arrange
        string resourceName = "Product";
        var resourceId = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(resourceName, resourceId);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetMessage()
    {
        // Arrange
        string customMessage = "Custom not found message";

        // Act
        var exception = new NotFoundException(customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetErrorCode()
    {
        // Arrange
        string customMessage = "Custom not found message";

        // Act
        var exception = new NotFoundException(customMessage);

        // Assert
        exception.ErrorCode.Should().Be("not_found");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetStatusCode()
    {
        // Arrange
        string customMessage = "Custom not found message";

        // Act
        var exception = new NotFoundException(customMessage);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("User", 123)]
    [InlineData("Order", "ABC-456")]
    [InlineData("Product", 999)]
    public void Constructor_WithVariousResourceTypes_ShouldFormatMessageCorrectly(string resourceName, object resourceId)
    {
        // Act
        var exception = new NotFoundException(resourceName, resourceId);

        // Assert
        exception.Message.Should().Be($"The {resourceName} with id '{resourceId}' was not found.");
    }
}
