using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Models;

public class CursorPaginatedResultTests
{
    private record TestDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt) : BaseDto(Id, CreatedAt, UpdatedAt);

    [Fact]
    public void Constructor_ShouldAllowSettingProperties()
    {
        // Arrange
        var items = new List<TestDto>
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow, null),
            new(Guid.NewGuid(), DateTimeOffset.UtcNow, null)
        };
        string nextCursor = Guid.NewGuid().ToString();

        // Act
        var result = new CursorPaginatedResult<TestDto>
        {
            Items = items,
            NextCursor = nextCursor
        };

        // Assert
        result.Items.Should().HaveCount(2);
        result.NextCursor.Should().Be(nextCursor);
    }

    [Fact]
    public void Items_ShouldBeSettable()
    {
        // Arrange
        var result = new CursorPaginatedResult<TestDto>();
        var items = new List<TestDto>
        {
            new(Guid.NewGuid(), DateTimeOffset.UtcNow, null)
        };

        // Act
        result.Items = items;

        // Assert
        result.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void NextCursor_ShouldBeSettable()
    {
        // Arrange
        var result = new CursorPaginatedResult<TestDto>();
        string cursor = Guid.NewGuid().ToString();

        // Act
        result.NextCursor = cursor;

        // Assert
        result.NextCursor.Should().Be(cursor);
    }

    [Fact]
    public void NextCursor_ShouldAcceptNull()
    {
        // Arrange
        var result = new CursorPaginatedResult<TestDto>
        {
            // Act
            NextCursor = null
        };

        // Assert
        result.NextCursor.Should().BeNull();
    }
}
