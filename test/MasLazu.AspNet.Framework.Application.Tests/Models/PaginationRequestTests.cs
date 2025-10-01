using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Models;

public class PaginationRequestTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new PaginationRequest();

        // Assert
        request.Page.Should().Be(1);
        request.PageSize.Should().Be(20);
        request.Filters.Should().NotBeNull().And.BeEmpty();
        request.OrderBy.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Page_ShouldBeSettable()
    {
        // Arrange
        var request = new PaginationRequest
        {
            // Act
            Page = 5
        };

        // Assert
        request.Page.Should().Be(5);
    }

    [Fact]
    public void PageSize_ShouldBeSettable()
    {
        // Arrange
        var request = new PaginationRequest
        {
            // Act
            PageSize = 50
        };

        // Assert
        request.PageSize.Should().Be(50);
    }

    [Fact]
    public void Filters_ShouldBeSettable()
    {
        // Arrange
        var request = new PaginationRequest();
        var filters = new List<Filter>
        {
            new() { Field = "Name", Operator = "=", Value = "Test" }
        };

        // Act
        request.Filters = filters;

        // Assert
        request.Filters.Should().BeEquivalentTo(filters);
    }

    [Fact]
    public void OrderBy_ShouldBeSettable()
    {
        // Arrange
        var request = new PaginationRequest();
        var orderBy = new List<OrderBy>
        {
            new() { Field = "CreatedAt", Desc = true }
        };

        // Act
        request.OrderBy = orderBy;

        // Assert
        request.OrderBy.Should().BeEquivalentTo(orderBy);
    }
}

public class FilterTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var filter = new Filter();

        // Assert
        filter.Field.Should().Be(string.Empty);
        filter.Operator.Should().Be("=");
        filter.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Field_ShouldBeSettable()
    {
        // Arrange
        var filter = new Filter
        {
            // Act
            Field = "Name"
        };

        // Assert
        filter.Field.Should().Be("Name");
    }

    [Fact]
    public void Operator_ShouldBeSettable()
    {
        // Arrange
        var filter = new Filter
        {
            // Act
            Operator = "contains"
        };

        // Assert
        filter.Operator.Should().Be("contains");
    }

    [Fact]
    public void Value_ShouldBeSettable()
    {
        // Arrange
        var filter = new Filter
        {
            // Act
            Value = "TestValue"
        };

        // Assert
        filter.Value.Should().Be("TestValue");
    }
}

public class OrderByTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var orderBy = new OrderBy();

        // Assert
        orderBy.Field.Should().Be(string.Empty);
        orderBy.Desc.Should().BeFalse();
    }

    [Fact]
    public void Field_ShouldBeSettable()
    {
        // Arrange
        var orderBy = new OrderBy
        {
            // Act
            Field = "CreatedAt"
        };

        // Assert
        orderBy.Field.Should().Be("CreatedAt");
    }

    [Fact]
    public void Desc_ShouldBeSettable()
    {
        // Arrange
        var orderBy = new OrderBy
        {
            // Act
            Desc = true
        };

        // Assert
        orderBy.Desc.Should().BeTrue();
    }
}
