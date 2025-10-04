using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Utils;
using MasLazu.AspNet.Framework.Domain.Entities;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Utils;

public class ExpressionBuilderTests
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
        Inactive,
        Pending
    }

    private readonly Mock<IEntityPropertyMap<TestEntity>> _propertyMapMock;
    private readonly ExpressionBuilder<TestEntity> _builder;
    private readonly List<TestEntity> _testData;

    public ExpressionBuilderTests()
    {
        _propertyMapMock = new Mock<IEntityPropertyMap<TestEntity>>();
        _builder = new ExpressionBuilder<TestEntity>(_propertyMapMock.Object);

        _testData = new List<TestEntity>
        {
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Alice", Age = 25, Price = 100.50m, IsActive = true, Status = TestStatus.Active },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Bob", Age = 30, Price = 200.75m, IsActive = false, Status = TestStatus.Inactive },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "Charlie", Age = 35, Price = 150.25m, IsActive = true, Status = TestStatus.Pending },
            new() { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "David", Age = 40, Price = 300.00m, IsActive = true, Status = TestStatus.Active }
        };
    }

    [Fact]
    public void ApplyFilters_WithEqualOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "=", Value = "Alice" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void ApplyFilters_WithNotEqualOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "!=", Value = "Alice" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().NotContain(e => e.Name == "Alice");
    }

    [Fact]
    public void ApplyFilters_WithGreaterThanOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var filters = new List<Filter>
        {
            new() { Field = "Age", Operator = ">", Value = "30" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Age.Should().BeGreaterThan(30));
    }

    [Fact]
    public void ApplyFilters_WithLessThanOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var filters = new List<Filter>
        {
            new() { Field = "Age", Operator = "<", Value = "30" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Age.Should().BeLessThan(30);
    }

    [Fact]
    public void ApplyFilters_WithGreaterThanOrEqualOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var filters = new List<Filter>
        {
            new() { Field = "Age", Operator = ">=", Value = "30" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(e => e.Age.Should().BeGreaterThanOrEqualTo(30));
    }

    [Fact]
    public void ApplyFilters_WithLessThanOrEqualOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var filters = new List<Filter>
        {
            new() { Field = "Age", Operator = "<=", Value = "30" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Age.Should().BeLessThanOrEqualTo(30));
    }

    [Fact]
    public void ApplyFilters_WithContainsOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "contains", Value = "li" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(2); // Alice and Charlie
        result.Should().AllSatisfy(e => e.Name.ToLower().Should().Contain("li"));
    }

    [Fact]
    public void ApplyFilters_WithStartsWithOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "startswith", Value = "C" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().StartWith("C");
    }

    [Fact]
    public void ApplyFilters_WithEndsWithOperator_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "endswith", Value = "e" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(2); // Alice and Charlie
        result.Should().AllSatisfy(e => e.Name.Should().EndWith("e"));
    }

    [Fact]
    public void ApplyFilters_WithMultipleFilters_ShouldApplyAllFilters()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        _propertyMapMock.Setup(m => m.Get("IsActive")).Returns(e => e.IsActive);
        var filters = new List<Filter>
        {
            new() { Field = "Age", Operator = ">=", Value = "30" },
            new() { Field = "IsActive", Operator = "=", Value = "true" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(2); // Charlie and David
        result.Should().AllSatisfy(e =>
        {
            e.Age.Should().BeGreaterThanOrEqualTo(30);
            e.IsActive.Should().BeTrue();
        });
    }

    [Fact]
    public void ApplyOrdering_WithSingleField_ShouldOrderAscending()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var orderBy = new List<OrderBy>
        {
            new() { Field = "Age", Desc = false }
        };

        // Act
        var result = _builder.ApplyOrdering(_testData.AsQueryable(), orderBy).ToList();

        // Assert
        result.Should().BeInAscendingOrder(e => e.Age);
    }

    [Fact]
    public void ApplyOrdering_WithSingleField_ShouldOrderDescending()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var orderBy = new List<OrderBy>
        {
            new() { Field = "Age", Desc = true }
        };

        // Act
        var result = _builder.ApplyOrdering(_testData.AsQueryable(), orderBy).ToList();

        // Assert
        result.Should().BeInDescendingOrder(e => e.Age);
    }

    [Fact]
    public void ApplyOrdering_WithMultipleFields_ShouldApplyThenBy()
    {
        // Arrange
        var moreTestData = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 25, IsActive = true },
            new() { Name = "Bob", Age = 25, IsActive = false },
            new() { Name = "Charlie", Age = 30, IsActive = true }
        };

        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);

        var orderBy = new List<OrderBy>
        {
            new() { Field = "Age", Desc = false },
            new() { Field = "Name", Desc = false }
        };

        // Act
        var result = _builder.ApplyOrdering(moreTestData.AsQueryable(), orderBy).ToList();

        // Assert
        result[0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public void ApplyPagination_ShouldApplyFiltersOrderingAndPaging()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 2,
            Filters = new List<Filter>(),
            OrderBy = new List<OrderBy>
            {
                new() { Field = "Age", Desc = false }
            }
        };

        // Act
        var result = _builder.ApplyPagination(_testData.AsQueryable(), request).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Age.Should().Be(25);
        result[1].Age.Should().Be(30);
    }

    [Fact]
    public void ApplyPagination_WithSecondPage_ShouldSkipFirstPage()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        var request = new PaginationRequest
        {
            Page = 2,
            PageSize = 2,
            Filters = new List<Filter>(),
            OrderBy = new List<OrderBy>
            {
                new() { Field = "Age", Desc = false }
            }
        };

        // Act
        var result = _builder.ApplyPagination(_testData.AsQueryable(), request).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Age.Should().Be(35);
        result[1].Age.Should().Be(40);
    }

    [Fact]
    public void ApplyCursor_WithNullCursor_ShouldReturnOriginalQuery()
    {
        // Arrange
        var request = new CursorPaginationRequest { Cursor = null };

        // Act
        var result = _builder.ApplyCursor(_testData.AsQueryable(), request).ToList();

        // Assert
        result.Should().HaveCount(4);
    }

    [Fact]
    public void ApplyCursor_WithEmptyGuidCursor_ShouldReturnOriginalQuery()
    {
        // Arrange
        var request = new CursorPaginationRequest { Cursor = Guid.Empty };

        // Act
        var result = _builder.ApplyCursor(_testData.AsQueryable(), request).ToList();

        // Assert
        result.Should().HaveCount(4);
    }

    [Fact]
    public void ApplyCursor_WithValidCursor_ShouldFilterByIdGreaterThan()
    {
        // Arrange
        Guid secondEntityId = _testData[1].Id;
        var request = new CursorPaginationRequest { Cursor = secondEntityId };

        // Act
        var result = _builder.ApplyCursor(_testData.AsQueryable(), request).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(e => e.Id.CompareTo(secondEntityId).Should().BeGreaterThan(0));
    }

    [Fact]
    public void ApplyFilters_WithUnsupportedOperator_ShouldThrowArgumentException()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "invalid", Value = "Test" }
        };

        // Act
        Action act = () => _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unsupported operator*");
    }

    [Fact]
    public void ApplyFilters_WithBooleanValue_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("IsActive")).Returns(e => e.IsActive);
        var filters = new List<Filter>
        {
            new() { Field = "IsActive", Operator = "=", Value = "true" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(e => e.IsActive.Should().BeTrue());
    }

    [Fact]
    public void ApplyFilters_WithEnumValue_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Status")).Returns(e => e.Status);
        var filters = new List<Filter>
        {
            new() { Field = "Status", Operator = "=", Value = "Active" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Status.Should().Be(TestStatus.Active));
    }

    [Fact]
    public void ApplyFilters_WithDecimalValue_ShouldFilterCorrectly()
    {
        // Arrange
        _propertyMapMock.Setup(m => m.Get("Price")).Returns(e => e.Price);
        var filters = new List<Filter>
        {
            new() { Field = "Price", Operator = ">", Value = "150" }
        };

        // Act
        var result = _builder.ApplyFilters(_testData.AsQueryable(), filters).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(e => e.Price.Should().BeGreaterThan(150));
    }
}
