using FluentAssertions;
using MasLazu.AspNet.Framework.Application.Models;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Models;

public class BaseDtoTests
{
    private record TestDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt) : BaseDto(Id, CreatedAt, UpdatedAt);

    [Fact]
    public void Constructor_ShouldSetId()
    {
        // Arrange
        var id = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new TestDto(id, createdAt, null);

        // Assert
        dto.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt()
    {
        // Arrange
        var id = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new TestDto(id, createdAt, null);

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Constructor_ShouldSetUpdatedAt_WhenNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        // Act
        var dto = new TestDto(id, createdAt, null);

        // Assert
        dto.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetUpdatedAt_WhenProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        var dto = new TestDto(id, createdAt, updatedAt);

        // Assert
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Equality_ShouldBeTrue_ForSameValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        var dto1 = new TestDto(id, createdAt, updatedAt);
        var dto2 = new TestDto(id, createdAt, updatedAt);

        // Assert
        dto1.Should().Be(dto2);
    }

    [Fact]
    public void Equality_ShouldBeFalse_ForDifferentIds()
    {
        // Arrange
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;

        // Act
        var dto1 = new TestDto(Guid.NewGuid(), createdAt, null);
        var dto2 = new TestDto(Guid.NewGuid(), createdAt, null);

        // Assert
        dto1.Should().NotBe(dto2);
    }
}
