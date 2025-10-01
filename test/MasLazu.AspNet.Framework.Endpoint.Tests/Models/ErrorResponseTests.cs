using FluentAssertions;
using MasLazu.AspNet.Framework.Endpoint.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Endpoint.Tests.Models;

public class ErrorResponseTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultProperties()
    {
        // Act
        var response = new ErrorResponse();

        // Assert
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(500);
        response.Code.Should().Be(string.Empty);
        response.Message.Should().Be(string.Empty);
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var response = new ErrorResponse();
        var errors = new { Field = "email", Message = "Invalid format" };

        // Act
        response.StatusCode = 400;
        response.Code = "validation_error";
        response.Message = "Validation failed";
        response.Errors = errors;

        // Assert
        response.StatusCode.Should().Be(400);
        response.Code.Should().Be("validation_error");
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().Be(errors);
    }

    [Fact]
    public void ErrorResponse_WithComplexErrors_ShouldStoreCorrectly()
    {
        // Arrange
        var complexErrors = new Dictionary<string, string[]>
        {
            ["email"] = new[] { "Email is required", "Email format is invalid" },
            ["password"] = new[] { "Password must be at least 8 characters" }
        };

        // Act
        var response = new ErrorResponse
        {
            StatusCode = 400,
            Code = "validation_error",
            Message = "Multiple validation errors",
            Errors = complexErrors
        };

        // Assert
        response.Errors.Should().BeEquivalentTo(complexErrors);
    }
}

public class ValidationErrorResponseTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var response = new ValidationErrorResponse();

        // Assert
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(400);
        response.Code.Should().Be("validation_error");
        response.Message.Should().Be("Validation failed");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void Errors_ShouldBeSettable()
    {
        // Arrange
        var response = new ValidationErrorResponse();
        var errors = new Dictionary<string, string[]>
        {
            ["name"] = new[] { "Name is required" },
            ["age"] = new[] { "Age must be positive" }
        };

        // Act
        response.Errors = errors;

        // Assert
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void ValidationErrorResponse_ShouldInheritFromErrorResponse()
    {
        // Act
        var response = new ValidationErrorResponse();

        // Assert
        response.Should().BeAssignableTo<ErrorResponse>();
    }

    [Fact]
    public void ValidationErrorResponse_WithMultipleErrors_ShouldStoreAll()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            ["field1"] = new[] { "Error 1", "Error 2", "Error 3" },
            ["field2"] = new[] { "Error A" },
            ["field3"] = new[] { "Error X", "Error Y" }
        };

        // Act
        var response = new ValidationErrorResponse
        {
            Errors = errors
        };

        // Assert
        response.Errors.Should().HaveCount(3);
        response.Errors["field1"].Should().HaveCount(3);
        response.Errors["field2"].Should().HaveCount(1);
        response.Errors["field3"].Should().HaveCount(2);
    }
}
