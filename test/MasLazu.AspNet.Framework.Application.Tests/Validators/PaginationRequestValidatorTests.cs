using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Validators;
using MasLazu.AspNet.Framework.Domain.Entities;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace MasLazu.AspNet.Framework.Application.Tests.Validators;

public class PaginationRequestValidatorTests
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public TestStatus Status { get; set; }
    }

    public enum TestStatus
    {
        Active,
        Inactive
    }

    private readonly Mock<IEntityPropertyMap<TestEntity>> _propertyMapMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly PaginationRequestValidator<TestEntity> _validator;

    public PaginationRequestValidatorTests()
    {
        _propertyMapMock = new Mock<IEntityPropertyMap<TestEntity>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Pagination:MaxPageSize"]).Returns("200");
        _validator = new PaginationRequestValidator<TestEntity>(_propertyMapMock.Object, _configurationMock.Object);

        // Setup property map for valid fields
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        _propertyMapMock.Setup(m => m.Get("Price")).Returns(e => e.Price);
        _propertyMapMock.Setup(m => m.Get("IsActive")).Returns(e => e.IsActive);
        _propertyMapMock.Setup(m => m.Get("Status")).Returns(e => e.Status);
        _propertyMapMock.Setup(m => m.Get("LastLoginAt")).Returns(e => e.LastLoginAt);
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>(),
            OrderBy = new List<OrderBy>()
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task Validate_WithInvalidPage_ShouldFail(int page)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity> { Page = page, PageSize = 20 };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(201)]
    [InlineData(500)]
    public async Task Validate_WithInvalidPageSize_ShouldFail(int pageSize)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity> { Page = 1, PageSize = pageSize };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public async Task Validate_WithValidPageSize_ShouldPass(int pageSize)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity> { Page = 1, PageSize = pageSize };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.PageSize);
    }

    [Fact]
    public async Task Validate_WithValidStringFilter_ShouldPass()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = "contains", Value = "test" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("=")]
    [InlineData("!=")]
    [InlineData("contains")]
    [InlineData("startswith")]
    [InlineData("endswith")]
    public async Task Validate_WithValidStringOperators_ShouldPass(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = op, Value = "test" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(">")]
    [InlineData("<")]
    [InlineData(">=")]
    [InlineData("<=")]
    public async Task Validate_WithInvalidStringOperators_ShouldFail(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = op, Value = "test" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Filters);
    }

    [Theory]
    [InlineData("=")]
    [InlineData("!=")]
    [InlineData(">")]
    [InlineData("<")]
    [InlineData(">=")]
    [InlineData("<=")]
    public async Task Validate_WithValidNumericOperators_ShouldPass(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Age", Operator = op, Value = "25" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("contains")]
    [InlineData("startswith")]
    [InlineData("endswith")]
    public async Task Validate_WithInvalidNumericOperators_ShouldFail(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Age", Operator = op, Value = "25" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Filters);
    }

    [Theory]
    [InlineData("=")]
    [InlineData("!=")]
    public async Task Validate_WithValidBooleanOperators_ShouldPass(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "IsActive", Operator = op, Value = "true" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(">")]
    [InlineData("contains")]
    public async Task Validate_WithInvalidBooleanOperators_ShouldFail(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "IsActive", Operator = op, Value = "true" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Filters);
    }

    [Theory]
    [InlineData("=")]
    [InlineData("!=")]
    public async Task Validate_WithValidEnumOperators_ShouldPass(string op)
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Status", Operator = op, Value = "Active" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithNonExistentField_ShouldFail()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("NonExistent"))
            .Throws(new KeyNotFoundException());

        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "NonExistent", Operator = "=", Value = "test" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Filters);
    }

    [Fact]
    public async Task Validate_WithEmptyFilterField_ShouldFail()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "", Operator = "=", Value = "test" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidOrderBy_ShouldPass()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "Name", Desc = false }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithNonExistentOrderByField_ShouldFail()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("NonExistent"))
            .Throws(new KeyNotFoundException());

        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "NonExistent", Desc = false }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyOrderByField_ShouldFail()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "", Desc = false }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Validate_WithMultipleFilters_ShouldValidateAll()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = "contains", Value = "test" },
                new() { Field = "Age", Operator = ">", Value = "25" },
                new() { Field = "IsActive", Operator = "=", Value = "true" }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithMultipleOrderBy_ShouldValidateAll()
    {
        // Arrange
        var request = new PaginationRequest<TestEntity>
        {
            Page = 1,
            PageSize = 20,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "Age", Desc = true },
                new() { Field = "Name", Desc = false }
            }
        };

        // Act
        TestValidationResult<PaginationRequest<TestEntity>> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
