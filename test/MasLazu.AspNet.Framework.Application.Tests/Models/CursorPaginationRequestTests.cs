using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Models;

public class CursorPaginationRequestTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var request = new CursorPaginationRequest();

        // Assert
        request.Limit.Should().Be(10);
        request.Cursor.Should().BeNull();
        request.Filters.Should().NotBeNull().And.BeEmpty();
        request.OrderBy.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Limit_ShouldBeSettable()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            // Act
            Limit = 50
        };

        // Assert
        request.Limit.Should().Be(50);
    }

    [Fact]
    public void Cursor_ShouldBeSettable()
    {
        // Arrange
        var request = new CursorPaginationRequest();
        var cursor = Guid.NewGuid();

        // Act
        request.Cursor = cursor;

        // Assert
        request.Cursor.Should().Be(cursor);
    }

    [Fact]
    public void Cursor_ShouldAcceptNull()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            // Act
            Cursor = null
        };

        // Assert
        request.Cursor.Should().BeNull();
    }

    [Fact]
    public void Filters_ShouldBeSettable()
    {
        // Arrange
        var request = new CursorPaginationRequest();
        var filters = new List<Filter>
        {
            new() { Field = "Status", Operator = "=", Value = "Active" }
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
        var request = new CursorPaginationRequest();
        var orderBy = new List<OrderBy>
        {
            new() { Field = "Id", Desc = false }
        };

        // Act
        request.OrderBy = orderBy;

        // Assert
        request.OrderBy.Should().BeEquivalentTo(orderBy);
    }
}
