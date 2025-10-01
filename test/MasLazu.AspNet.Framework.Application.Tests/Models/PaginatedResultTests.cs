using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Models;

public class PaginatedResultTests
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

        // Act
        var result = new PaginatedResult<TestDto>
        {
            TotalCount = 100,
            PageSize = 20,
            Page = 1,
            Items = items
        };

        // Assert
        result.TotalCount.Should().Be(100);
        result.PageSize.Should().Be(20);
        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Items_ShouldBeSettable()
    {
        // Arrange
        var result = new PaginatedResult<TestDto>();
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
    public void TotalCount_ShouldBeSettable()
    {
        // Arrange
        var result = new PaginatedResult<TestDto>
        {
            // Act
            TotalCount = 150
        };

        // Assert
        result.TotalCount.Should().Be(150);
    }

    [Fact]
    public void PageSize_ShouldBeSettable()
    {
        // Arrange
        var result = new PaginatedResult<TestDto>
        {
            // Act
            PageSize = 50
        };

        // Assert
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public void Page_ShouldBeSettable()
    {
        // Arrange
        var result = new PaginatedResult<TestDto>
        {
            // Act
            Page = 3
        };

        // Assert
        result.Page.Should().Be(3);
    }
}
