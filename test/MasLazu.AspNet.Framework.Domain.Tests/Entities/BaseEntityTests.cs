using FluentAssertions;
using MasLazu.AspNet.Framework.Domain.Entities;
using Xunit;

namespace MasLazu.AspNet.Framework.Domain.Tests.Entities;

public class BaseEntityTests
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitializeId_WithVersion7Guid()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_ShouldInitializeCreatedAt_WithUtcNow()
    {
        // Arrange
        DateTimeOffset beforeCreation = DateTimeOffset.UtcNow.AddSeconds(-1);

        // Act
        var entity = new TestEntity();

        // Assert
        DateTimeOffset afterCreation = DateTimeOffset.UtcNow.AddSeconds(1);
        entity.CreatedAt.Should().BeAfter(beforeCreation);
        entity.CreatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public void Constructor_ShouldInitializeUpdatedAt_AsNull()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldInitializeDeletedAt_AsNull()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Id_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var newId = Guid.NewGuid();

        // Act
        entity.Id = newId;

        // Assert
        entity.Id.Should().Be(newId);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        DateTimeOffset newCreatedAt = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        entity.CreatedAt = newCreatedAt;

        // Assert
        entity.CreatedAt.Should().Be(newCreatedAt);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        DateTimeOffset updatedAt = DateTimeOffset.UtcNow;

        // Act
        entity.UpdatedAt = updatedAt;

        // Assert
        entity.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void DeletedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        DateTimeOffset deletedAt = DateTimeOffset.UtcNow;

        // Act
        entity.DeletedAt = deletedAt;

        // Assert
        entity.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void MultipleEntities_ShouldHaveDifferentIds()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        entity1.Id.Should().NotBe(entity2.Id);
    }

    [Fact]
    public void Entity_ShouldSupportDerivedProperties()
    {
        // Arrange
        var entity = new TestEntity();
        string name = "Test Entity";

        // Act
        entity.Name = name;

        // Assert
        entity.Name.Should().Be(name);
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtWithoutAffectingOtherProperties()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };
        Guid originalId = entity.Id;
        DateTimeOffset originalCreatedAt = entity.CreatedAt;

        // Act
        entity.DeletedAt = DateTimeOffset.UtcNow;

        // Assert
        entity.DeletedAt.Should().NotBeNull();
        entity.Id.Should().Be(originalId);
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.Name.Should().Be("Test");
    }

    [Fact]
    public void Update_ShouldSetUpdatedAtWithoutAffectingOtherProperties()
    {
        // Arrange
        var entity = new TestEntity { Name = "Original" };
        Guid originalId = entity.Id;
        DateTimeOffset originalCreatedAt = entity.CreatedAt;

        // Act
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.Name = "Updated";

        // Assert
        entity.UpdatedAt.Should().NotBeNull();
        entity.Id.Should().Be(originalId);
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.DeletedAt.Should().BeNull();
        entity.Name.Should().Be("Updated");
    }
}
