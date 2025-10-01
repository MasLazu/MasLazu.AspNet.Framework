using FluentAssertions;
using MasLazu.AspNet.Framework.Endpoint.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Endpoint.Tests.Models;

public class SuccessResponseTests
{
    [Fact]
    public void CreateSuccess_ShouldSetDataAndMessage()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test" };
        string message = "Success";

        // Act
        var response = SuccessResponse<object>.CreateSuccess(data, message);

        // Assert
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void CreateSuccess_WithStatusCode_ShouldSetStatusCode()
    {
        // Arrange
        var data = new { Id = 1 };
        string message = "Created";
        int statusCode = 201;

        // Act
        var response = SuccessResponse<object>.CreateSuccess(data, message, statusCode);

        // Assert
        response.StatusCode.Should().Be(statusCode);
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void CreateSuccess_WithoutMessage_ShouldHaveDefaultMessage()
    {
        // Arrange
        var data = new { Id = 1 };

        // Act
        var response = SuccessResponse<object>.CreateSuccess(data);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be("Operation completed successfully");
        response.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(202)]
    [InlineData(204)]
    public void CreateSuccess_WithVariousStatusCodes_ShouldSetCorrectly(int statusCode)
    {
        // Arrange
        string data = "test data";

        // Act
        var response = SuccessResponse<string>.CreateSuccess(data, statusCode: statusCode);

        // Assert
        response.StatusCode.Should().Be(statusCode);
    }
}
