using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.Application.Utils;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Repositories;
using Moq;
using Xunit;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;

namespace MasLazu.AspNet.Framework.EfCore.Tests.Repositories;

public class ReadRepositoryTests : IDisposable
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    public class TestReadDbContext : BaseReadDbContext
    {
        public TestReadDbContext(DbContextOptions<TestReadDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>().ToTable("TestEntities");
        }
    }

    // Writable context for seeding test data
    public class TestWriteDbContext : BaseDbContext
    {
        public TestWriteDbContext(DbContextOptions<TestWriteDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>().ToTable("TestEntities");
        }
    }

    private readonly string _databaseName;
    public readonly TestReadDbContext _context;
    public readonly ReadRepository<TestEntity, TestReadDbContext> _repository;
    public readonly Mock<IEntityPropertyMap<TestEntity>> _propertyMapMock;

    public ReadRepositoryTests()
    {
        _databaseName = Guid.NewGuid().ToString();

        DbContextOptions<TestReadDbContext> options = new DbContextOptionsBuilder<TestReadDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        _context = new TestReadDbContext(options);
        _propertyMapMock = new Mock<IEntityPropertyMap<TestEntity>>();

        var expressionBuilder = new ExpressionBuilder<TestEntity>(_propertyMapMock.Object);
        _repository = new ReadRepository<TestEntity, TestReadDbContext>(_context, expressionBuilder);

        // Setup property map
        _propertyMapMock.Setup(m => m.Get("Name")).Returns(e => e.Name);
        _propertyMapMock.Setup(m => m.Get("Age")).Returns(e => e.Age);
        _propertyMapMock.Setup(m => m.Get("IsActive")).Returns(e => e.IsActive);
        _propertyMapMock.Setup(m => m.Get("Id")).Returns(e => e.Id);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task SeedData()
    {
        // Use a writable context to seed data
        DbContextOptions<TestWriteDbContext> writeOptions = new DbContextOptionsBuilder<TestWriteDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        using var writeContext = new TestWriteDbContext(writeOptions);

        var entities = new List<TestEntity>
        {
            new() { Name = "Alice", Age = 25, IsActive = true },
            new() { Name = "Bob", Age = 30, IsActive = false },
            new() { Name = "Charlie", Age = 35, IsActive = true },
            new() { Name = "David", Age = 40, IsActive = true, DeletedAt = DateTimeOffset.UtcNow }
        };

        writeContext.TestEntities.AddRange(entities);
        await writeContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnEntity()
    {
        // Arrange
        await SeedData();

        // Use writable context to get the entity ID
        DbContextOptions<TestWriteDbContext> writeOptions = new DbContextOptionsBuilder<TestWriteDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        Guid entityId;
        using (var writeContext = new TestWriteDbContext(writeOptions))
        {
            TestEntity entity = await writeContext.TestEntities.FirstAsync(e => e.Name == "Alice");
            entityId = entity.Id;
        }

        // Act
        TestEntity? result = await _repository.GetByIdAsync(entityId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDeleted_ShouldReturnNull()
    {
        // Arrange
        await SeedData();

        // Use writable context to get the entity ID
        DbContextOptions<TestWriteDbContext> writeOptions = new DbContextOptionsBuilder<TestWriteDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        Guid entityId;
        using (var writeContext = new TestWriteDbContext(writeOptions))
        {
            TestEntity entity = await writeContext.TestEntities.FirstAsync(e => e.Name == "David");
            entityId = entity.Id;
        }

        // Act
        TestEntity? result = await _repository.GetByIdAsync(entityId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyNonDeletedEntities()
    {
        // Arrange
        await SeedData();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().NotContain(e => e.Name == "David");
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        await SeedData();

        // Act
        var result = (await _repository.FindAsync(e => e.IsActive)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WhenMatch_ShouldReturnEntity()
    {
        // Arrange
        await SeedData();

        // Act
        TestEntity? result = await _repository.FirstOrDefaultAsync(e => e.Name == "Bob");

        // Assert
        result.Should().NotBeNull();
        result!.Age.Should().Be(30);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_WhenNoMatch_ShouldReturnNull()
    {
        // Arrange
        await SeedData();

        // Act
        TestEntity? result = await _repository.FirstOrDefaultAsync(e => e.Name == "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityExists_ShouldReturnTrue()
    {
        // Arrange
        await SeedData();

        // Use writable context to get the entity ID
        DbContextOptions<TestWriteDbContext> writeOptions = new DbContextOptionsBuilder<TestWriteDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        Guid entityId;
        using (var writeContext = new TestWriteDbContext(writeOptions))
        {
            TestEntity entity = await writeContext.TestEntities.FirstAsync(e => e.Name == "Alice");
            entityId = entity.Id;
        }

        // Act
        bool result = await _repository.ExistsAsync(entityId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityDeleted_ShouldReturnFalse()
    {
        // Arrange
        await SeedData();

        // Use writable context to get the entity ID
        DbContextOptions<TestWriteDbContext> writeOptions = new DbContextOptionsBuilder<TestWriteDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        Guid entityId;
        using (var writeContext = new TestWriteDbContext(writeOptions))
        {
            TestEntity entity = await writeContext.TestEntities.FirstAsync(e => e.Name == "David");
            entityId = entity.Id;
        }

        // Act
        bool result = await _repository.ExistsAsync(entityId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCountOfNonDeletedEntities()
    {
        // Arrange
        await SeedData();

        // Act
        int result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnMatchingCount()
    {
        // Arrange
        await SeedData();

        // Act
        int result = await _repository.CountAsync(e => e.IsActive);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetPaginatedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        await SeedData();

        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 2,
            OrderBy = new List<OrderBy> { new() { Field = "Age", Desc = false } }
        };

        // Act
        (List<TestEntity>? items, int totalCount) = await _repository.GetPaginatedAsync(request);

        // Assert
        items.Should().HaveCount(2);
        totalCount.Should().Be(3);
        items.First().Name.Should().Be("Alice");
        items.Last().Name.Should().Be("Bob");
    }

    [Fact]
    public async Task GetCursorPaginatedAsync_ShouldReturnCorrectResults()
    {
        // Arrange
        await SeedData();

        var request = new CursorPaginationRequest
        {
            Limit = 2,
            OrderBy = new List<OrderBy> { new() { Field = "Age", Desc = false } }
        };

        // Act
        (List<TestEntity>? items, Guid? nextCursor) = await _repository.GetCursorPaginatedAsync(request);

        // Assert
        items.Should().HaveCount(2);
        nextCursor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        await SeedData();
        // Use writable context to query seed data for IDs
        DbContextOptions<TestWriteDbContext> writeOptions = new DbContextOptionsBuilder<TestWriteDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;

        List<Guid> ids;
        using (var writeContext = new TestWriteDbContext(writeOptions))
        {
            List<TestEntity> allEntities = await writeContext.TestEntities.Where(e => e.DeletedAt == null).ToListAsync();
            ids = allEntities.Take(2).Select(e => e.Id).ToList();
        }

        // Act
        List<TestEntity> result = await _repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => ids.Contains(e.Id));
    }
}
