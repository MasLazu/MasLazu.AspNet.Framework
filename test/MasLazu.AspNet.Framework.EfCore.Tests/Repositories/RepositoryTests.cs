using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Utils;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Repositories;
using Moq;
using Xunit;

namespace MasLazu.AspNet.Framework.EfCore.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class TestDbContext : BaseDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>().ToTable("TestEntities");
        }
    }

    private readonly TestDbContext _context;
    private readonly Repository<TestEntity, TestDbContext> _repository;
    private readonly Mock<IEntityPropertyMap<TestEntity>> _propertyMapMock;

    public RepositoryTests()
    {
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _propertyMapMock = new Mock<IEntityPropertyMap<TestEntity>>();

        var expressionBuilder = new ExpressionBuilder<TestEntity>(_propertyMapMock.Object);
        _repository = new Repository<TestEntity, TestDbContext>(_context, expressionBuilder);

        // Setup property map
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        _propertyMapMock.Setup(m => m.Get("Price")).Returns(e => e.Price);
        _propertyMapMock.Setup(m => m.Get("IsActive")).Returns(e => e.IsActive);
        _propertyMapMock.Setup(m => m.Get("Id")).Returns(e => e.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };

        // Act
        TestEntity result = await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeNull();
        result.DeletedAt.Should().BeNull();

        TestEntity? savedEntity = await _context.TestEntities.FindAsync(result.Id);
        savedEntity.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _repository.AddAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 25 },
            new() { Name = "Test2", Age = 30 }
        };

        // Act
        var result = (await _repository.AddRangeAsync(entities)).ToList();
        await _context.SaveChangesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e =>
        {
            e.Id.Should().NotBe(Guid.Empty);
            e.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        });

        int savedCount = await _context.TestEntities.CountAsync();
        savedCount.Should().Be(2);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        TestEntity? result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDeleted_ShouldReturnNull()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Soft delete the entity
        await _repository.DeleteAsync(entity.Id);
        await _context.SaveChangesAsync();

        // Act
        TestEntity? result = await _repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Act
        TestEntity? result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdsAsync Tests

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entity1 = new TestEntity { Name = "Test1", Age = 25 };
        var entity2 = new TestEntity { Name = "Test2", Age = 30 };
        var entity3 = new TestEntity { Name = "Test3", Age = 35 };

        await _repository.AddAsync(entity1);
        await _repository.AddAsync(entity2);
        await _repository.AddAsync(entity3);
        await _context.SaveChangesAsync();

        var ids = new List<Guid> { entity1.Id, entity3.Id };

        // Act
        List<TestEntity> result = await _repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(e => e.Id == entity1.Id);
        result.Should().Contain(e => e.Id == entity3.Id);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var entity1 = new TestEntity { Name = "Test1", Age = 25 };
        var entity2 = new TestEntity { Name = "Test2", Age = 30 };

        await _repository.AddAsync(entity1);
        await _repository.AddAsync(entity2);
        await _context.SaveChangesAsync();

        // Soft delete entity2
        await _repository.DeleteAsync(entity2.Id);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Test1");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Original", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        entity.Name = "Updated";
        entity.Age = 30;
        await _repository.UpdateAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        TestEntity? updated = await _repository.GetByIdAsync(entity.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated");
        updated.Age.Should().Be(30);
        updated.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _repository.UpdateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UpdateRangeAsync Tests

    [Fact]
    public async Task UpdateRangeAsync_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 25 },
            new() { Name = "Test2", Age = 30 }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        entities[0].Name = "Updated1";
        entities[1].Name = "Updated2";
        await _repository.UpdateRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Assert
        TestEntity? updated1 = await _repository.GetByIdAsync(entities[0].Id);
        TestEntity? updated2 = await _repository.GetByIdAsync(entities[1].Id);

        updated1!.Name.Should().Be("Updated1");
        updated2!.Name.Should().Be("Updated2");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity);
        await _context.SaveChangesAsync();

        // Assert
        TestEntity? deleted = await _repository.GetByIdAsync(entity.Id);
        deleted.Should().BeNull();

        TestEntity? rawEntity = await _context.TestEntities.IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == entity.Id);
        rawEntity.Should().NotBeNull();
        rawEntity!.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ById_ShouldSoftDeleteEntity()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity.Id);
        await _context.SaveChangesAsync();

        // Assert
        TestEntity? deleted = await _repository.GetByIdAsync(entity.Id);
        deleted.Should().BeNull();
    }

    #endregion

    #region DeleteRangeAsync Tests

    [Fact]
    public async Task DeleteRangeAsync_ShouldSoftDeleteMultipleEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 25 },
            new() { Name = "Test2", Age = 30 }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Assert
        var remaining = (await _repository.GetAllAsync()).ToList();
        remaining.Should().BeEmpty();

        int rawCount = await _context.TestEntities.IgnoreQueryFilters().CountAsync();
        rawCount.Should().Be(2);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenEntityExists_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.ExistsAsync(entity.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityDeleted_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();
        await _repository.DeleteAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        bool result = await _repository.ExistsAsync(entity.Id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_ShouldReturnCountOfNonDeletedEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 25 },
            new() { Name = "Test2", Age = 30 },
            new() { Name = "Test3", Age = 35 }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Soft delete entity2
        await _repository.DeleteAsync(entities[1].Id);
        await _context.SaveChangesAsync();

        // Act
        int result = await _repository.CountAsync();

        // Assert
        result.Should().Be(2);
    }

    #endregion

    #region FindAsync Tests

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 25 },
            new() { Name = "Bob", Age = 30 },
            new() { Name = "Charlie", Age = 25 }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.FindAsync(e => e.Age == 25)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Age.Should().Be(25));
    }

    #endregion

    #region FirstOrDefaultAsync Tests

    [Fact]
    public async Task FirstOrDefaultAsync_WhenEntityExists_ShouldReturnFirst()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 25 },
            new() { Name = "Bob", Age = 25 }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        // Act
        TestEntity? result = await _repository.FirstOrDefaultAsync(e => e.Age == 25);

        // Assert
        result.Should().NotBeNull();
        result!.Age.Should().Be(25);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WhenNoMatch_ShouldReturnNull()
    {
        // Arrange
        var entity = new TestEntity { Name = "Alice", Age = 25 };
        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        TestEntity? result = await _repository.FirstOrDefaultAsync(e => e.Age == 99);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var entities = new List<TestEntity>();
        for (int i = 1; i <= 50; i++)
        {
            entities.Add(new TestEntity { Name = $"Test{i}", Age = 20 + i });
        }

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var request = new PaginationRequest
        {
            Page = 2,
            PageSize = 10,
            OrderBy = new List<OrderBy> { new() { Field = "Age", Desc = false } }
        };

        // Act
        (List<TestEntity>? items, int totalCount) = await _repository.GetPaginatedAsync(request);

        // Assert
        items.Should().HaveCount(10);
        totalCount.Should().Be(50);
        items.First().Age.Should().Be(31); // Age 21-30 on page 1, 31-40 on page 2
    }

    [Fact]
    public async Task GetPaginatedAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 25, IsActive = true },
            new() { Name = "Bob", Age = 30, IsActive = false },
            new() { Name = "Charlie", Age = 35, IsActive = true }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 10,
            Filters = new List<Filter>
            {
                new() { Field = "IsActive", Operator = "=", Value = "true" }
            }
        };

        // Act
        (List<TestEntity>? items, int totalCount) = await _repository.GetPaginatedAsync(request);

        // Assert
        items.Should().HaveCount(2);
        totalCount.Should().Be(2);
        items.Should().AllSatisfy(e => e.IsActive.Should().BeTrue());
    }

    #endregion

    #region GetCursorPaginatedAsync Tests

    [Fact]
    public async Task GetCursorPaginatedAsync_ShouldReturnCursorPaginatedResults()
    {
        // Arrange
        var entities = new List<TestEntity>();
        for (int i = 1; i <= 15; i++)
        {
            entities.Add(new TestEntity { Name = $"Test{i}", Age = 20 + i });
        }

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var request = new CursorPaginationRequest
        {
            Limit = 5,
            OrderBy = new List<OrderBy> { new() { Field = "Age", Desc = false } }
        };

        // Act
        (List<TestEntity>? items, Guid? nextCursor) = await _repository.GetCursorPaginatedAsync(request);

        // Assert
        items.Should().HaveCount(5);
        nextCursor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCursorPaginatedAsync_WithCursor_ShouldReturnNextPage()
    {
        // Arrange
        var entities = new List<TestEntity>();
        for (int i = 1; i <= 10; i++)
        {
            entities.Add(new TestEntity { Name = $"Test{i}", Age = 20 + i });
        }

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var sortedEntities = entities.OrderBy(e => e.Id).ToList();
        var firstPageRequest = new CursorPaginationRequest
        {
            Limit = 3,
            OrderBy = new List<OrderBy> { new() { Field = "Id", Desc = false } }
        };

        (List<TestEntity>? firstPage, Guid? cursor) = await _repository.GetCursorPaginatedAsync(firstPageRequest);

        var secondPageRequest = new CursorPaginationRequest
        {
            Limit = 3,
            Cursor = cursor,
            OrderBy = new List<OrderBy> { new() { Field = "Id", Desc = false } }
        };

        // Act
        (List<TestEntity>? secondPage, Guid? _) = await _repository.GetCursorPaginatedAsync(secondPageRequest);

        // Assert
        firstPage.Should().HaveCount(3);
        secondPage.Should().HaveCount(3);
        secondPage.Should().AllSatisfy(e => string.Compare(e.Id.ToString(), firstPage.Last().Id.ToString(), StringComparison.Ordinal).Should().BeGreaterThan(0));
    }

    [Fact]
    public async Task GetCursorPaginatedAsync_LastPage_ShouldReturnNullCursor()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 25 },
            new() { Name = "Test2", Age = 30 }
        };

        await _repository.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        var request = new CursorPaginationRequest
        {
            Limit = 10,
            OrderBy = new List<OrderBy> { new() { Field = "Id", Desc = false } }
        };

        // Act
        (List<TestEntity>? items, Guid? nextCursor) = await _repository.GetCursorPaginatedAsync(request);

        // Assert
        items.Should().HaveCount(2);
        nextCursor.Should().BeNull();
    }

    #endregion

    #region GetTimeseriesCountAsync Tests

    [Fact]
    public async Task GetTimeseriesCountAsync_ShouldGroupByInterval()
    {
        // Arrange
        DateTime baseDate = DateTimeOffset.UtcNow.Date;
        var entities = new List<TestEntity>
        {
            new() { Name = "Test1", Age = 25, CreatedAt = baseDate.AddHours(1) },
            new() { Name = "Test2", Age = 30, CreatedAt = baseDate.AddHours(2) },
            new() { Name = "Test3", Age = 35, CreatedAt = baseDate.AddHours(2.5) },
            new() { Name = "Test4", Age = 40, CreatedAt = baseDate.AddHours(5) }
        };

        foreach (TestEntity entity in entities)
        {
            _context.TestEntities.Add(entity);
        }
        await _context.SaveChangesAsync();

        var timeRange = new TimeRange
        {
            Start = baseDate,
            End = baseDate.AddHours(24)
        };

        // Act
        var result = (await _repository.GetTimeseriesCountAsync(timeRange, TimeSpan.FromHours(3))).ToList();

        // Assert
        result.Should().NotBeEmpty();
        result.Sum(r => r.Count).Should().Be(4);
    }

    #endregion
}
