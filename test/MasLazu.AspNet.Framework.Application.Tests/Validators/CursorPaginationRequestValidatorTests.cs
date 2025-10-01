using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Validators;
using MasLazu.AspNet.Framework.Domain.Entities;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Validators;

public class CursorPaginationRequestValidatorTests
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    private readonly Mock<IEntityPropertyMap<TestEntity>> _propertyMapMock;
    private readonly CursorPaginationRequestValidator<TestEntity> _validator;

    public CursorPaginationRequestValidatorTests()
    {
        _propertyMapMock = new Mock<IEntityPropertyMap<TestEntity>>();
        _validator = new CursorPaginationRequestValidator<TestEntity>(_propertyMapMock.Object);

        // Setup property map for valid fields
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        _propertyMapMock.Setup(m => m.Get("IsActive")).Returns(e => e.IsActive);
        _propertyMapMock.Setup(m => m.Get("Id")).Returns(e => e.Id);
    }

    [Fact]
    public async Task Validate_WithValidRequest_ShouldPass()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Cursor = null,
            Filters = new List<Filter>(),
            OrderBy = new List<OrderBy>()
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(201)]
    [InlineData(500)]
    public async Task Validate_WithInvalidLimit_ShouldFail(int limit)
    {
        // Arrange
        var request = new CursorPaginationRequest { Limit = limit };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Limit);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public async Task Validate_WithValidLimit_ShouldPass(int limit)
    {
        // Arrange
        var request = new CursorPaginationRequest { Limit = limit };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.Limit);
    }

    [Fact]
    public async Task Validate_WithNullCursor_ShouldPass()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Cursor = null
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.Cursor);
    }

    [Fact]
    public async Task Validate_WithValidCursor_ShouldPass()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Cursor = Guid.NewGuid()
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.Cursor);
    }

    [Fact]
    public async Task Validate_WithEmptyGuidCursor_ShouldFail()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Cursor = Guid.Empty
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Cursor);
    }

    [Fact]
    public async Task Validate_WithValidFilter_ShouldPass()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = "contains", Value = "test" }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

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
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = op, Value = "test" }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(">")]
    [InlineData("<")]
    public async Task Validate_WithInvalidStringOperators_ShouldFail(string op)
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = op, Value = "test" }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

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
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Filters = new List<Filter>
            {
                new() { Field = "Age", Operator = op, Value = "25" }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithNonExistentFilterField_ShouldFail()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("NonExistent"))
            .Throws(new KeyNotFoundException());

        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Filters = new List<Filter>
            {
                new() { Field = "NonExistent", Operator = "=", Value = "test" }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Filters);
    }

    [Fact]
    public async Task Validate_WithEmptyFilterField_ShouldFail()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            Filters = new List<Filter>
            {
                new() { Field = "", Operator = "=", Value = "test" }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidOrderBy_ShouldPass()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "Id", Desc = false }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WithNonExistentOrderByField_ShouldFail()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("NonExistent"))
            .Throws(new KeyNotFoundException());

        var request = new CursorPaginationRequest
        {
            Limit = 10,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "NonExistent", Desc = false }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Validate_WithEmptyOrderByField_ShouldFail()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10,
            OrderBy = new List<OrderBy>
            {
                new() { Field = "", Desc = false }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Validate_WithMultipleFiltersAndOrderBy_ShouldValidateAll()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 20,
            Cursor = Guid.NewGuid(),
            Filters = new List<Filter>
            {
                new() { Field = "Name", Operator = "contains", Value = "test" },
                new() { Field = "Age", Operator = ">", Value = "25" }
            },
            OrderBy = new List<OrderBy>
            {
                new() { Field = "Age", Desc = true },
                new() { Field = "Id", Desc = false }
            }
        };

        // Act
        TestValidationResult<CursorPaginationRequest> result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
