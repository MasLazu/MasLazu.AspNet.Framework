using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MasLazu.AspNet.Framework.Application.Exceptions;
using MasLazu.AspNet.Framework.Endpoint.Middlewares;
using MasLazu.AspNet.Framework.Endpoint.Models;
using Xunit;
using FastEndpoints;
using FastEndpointsErrorResponse = FastEndpoints.ErrorResponse;

namespace MasLazu.AspNet.Framework.Endpoint.Tests.Middlewares;

public class ExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlerMiddleware>> _loggerMock;
    private readonly ExceptionHandlerMiddleware _middleware;
    private readonly DefaultHttpContext _httpContext;

    public ExceptionHandlerMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandlerMiddleware>>();
        _middleware = new ExceptionHandlerMiddleware(
            _ => Task.CompletedTask,
            _loggerMock.Object
        );
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        // Arrange
        bool nextCalled = false;
        var middleware = new ExceptionHandlerMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenAppException_ShouldHandleAndReturnErrorResponse()
    {
        // Arrange
        var exception = new BadRequestException("Invalid data");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        _httpContext.Response.ContentType.Should().Be("application/json");

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        Endpoint.Models.ErrorResponse? errorResponse = JsonSerializer.Deserialize<Endpoint.Models.ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(400);
        errorResponse.Code.Should().Be("bad_request");
        errorResponse.Message.Should().Be("Invalid data");
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Arrange
        var exception = new NotFoundException("User", Guid.NewGuid());
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        Endpoint.Models.ErrorResponse? errorResponse = JsonSerializer.Deserialize<Endpoint.Models.ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Code.Should().Be("not_found");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedException_ShouldReturn401()
    {
        // Arrange
        var exception = new UnauthorizedException("Not authenticated");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_WhenForbiddenException_ShouldReturn403()
    {
        // Arrange
        var exception = new ForbiddenException("Access denied");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WhenConflictException_ShouldReturn409()
    {
        // Arrange
        var exception = new ConflictException("Resource already exists");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task InvokeAsync_WhenValidationFailureException_ShouldReturn400WithErrors()
    {
        // Arrange
        var validationFailures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Age", "Age must be positive"),
            new("Age", "Age must be less than 150")
        };

        var exception = new ValidationFailureException(validationFailures, "Validation failed");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(400);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        ValidationErrorResponse? errorResponse = JsonSerializer.Deserialize<ValidationErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Errors.Should().ContainKey("Name");
        errorResponse.Errors.Should().ContainKey("Age");
        errorResponse.Errors["Name"].Should().HaveCount(1);
        errorResponse.Errors["Age"].Should().HaveCount(2);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedException_ShouldReturn500()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected error");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.StatusCode.Should().Be(500);

        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        Endpoint.Models.ErrorResponse? errorResponse = JsonSerializer.Deserialize<Endpoint.Models.ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.StatusCode.Should().Be(500);
        errorResponse.Code.Should().Be("internal_server_error");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogAppException()
    {
        // Arrange
        var exception = new BadRequestException("Test error");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogUnexpectedException()
    {
        // Arrange
        var exception = new InvalidOperationException("Unexpected");
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAppExceptionDetails_ShouldIncludeInResponse()
    {
        // Arrange
        var details = new { Field = "email", Value = "invalid@" };
        var exception = new BadRequestException("Invalid email", details);
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw exception,
            _loggerMock.Object
        );

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        string responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        Endpoint.Models.ErrorResponse? errorResponse = JsonSerializer.Deserialize<Endpoint.Models.ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        errorResponse.Should().NotBeNull();
        errorResponse!.Errors.Should().NotBeNull();
    }
}
