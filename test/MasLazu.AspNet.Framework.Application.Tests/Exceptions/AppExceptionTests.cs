using System.Net;
using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Exceptions;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Exceptions;

public class AppExceptionTests
{
    private class TestAppException : AppException
    {
        public TestAppException(string message, string errorCode, HttpStatusCode statusCode, object? details = null)
            : base(message, errorCode, statusCode, details)
        {
        }
    }

    [Fact]
    public void Constructor_ShouldSetMessage()
    {
        // Arrange
        string message = "Test error message";

        // Act
        var exception = new TestAppException(message, "test_error", HttpStatusCode.BadRequest);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Arrange
        string errorCode = "test_error_code";

        // Act
        var exception = new TestAppException("Test", errorCode, HttpStatusCode.BadRequest);

        // Assert
        exception.ErrorCode.Should().Be(errorCode);
    }

    [Fact]
    public void Constructor_ShouldSetStatusCode()
    {
        // Arrange
        HttpStatusCode statusCode = HttpStatusCode.NotFound;

        // Act
        var exception = new TestAppException("Test", "not_found", statusCode);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
    }

    [Fact]
    public void Constructor_ShouldSetDetails_WhenProvided()
    {
        // Arrange
        var details = new { Field = "Value", Count = 5 };

        // Act
        var exception = new TestAppException("Test", "test_error", HttpStatusCode.BadRequest, details);

        // Assert
        exception.Details.Should().Be(details);
    }

    [Fact]
    public void Constructor_ShouldSetDetailsToNull_WhenNotProvided()
    {
        // Act
        var exception = new TestAppException("Test", "test_error", HttpStatusCode.BadRequest);

        // Assert
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldDefaultToInternalServerError_WhenStatusCodeNotSpecified()
    {
        // Act
        var exception = new TestAppException("Test", "test_error", HttpStatusCode.InternalServerError);

        // Assert
        exception.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
