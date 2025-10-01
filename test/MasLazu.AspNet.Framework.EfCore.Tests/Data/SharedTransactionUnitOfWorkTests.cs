using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.EfCore.Data;
using Xunit;

namespace MasLazu.AspNet.Framework.EfCore.Tests.Data;

public class SharedTransactionUnitOfWorkTests : IDisposable
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestDbContext1 : BaseDbContext
    {
        public TestDbContext1(DbContextOptions<TestDbContext1> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>().ToTable("TestEntities1");
        }
    }

    public class TestDbContext2 : BaseDbContext
    {
        public TestDbContext2(DbContextOptions<TestDbContext2> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TestEntity>().ToTable("TestEntities2");
        }
    }

    private readonly SqliteConnection _connection;
    private readonly TestDbContext1 _context1;
    private readonly TestDbContext2 _context2;
    private readonly SharedTransactionUnitOfWork _unitOfWork;

    public SharedTransactionUnitOfWorkTests()
    {
        // Use SQLite in-memory database which supports transactions
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        DbContextOptions<TestDbContext1> options1 = new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options;

        DbContextOptions<TestDbContext2> options2 = new DbContextOptionsBuilder<TestDbContext2>()
            .UseSqlite(_connection)
            .Options;

        _context1 = new TestDbContext1(options1);
        _context2 = new TestDbContext2(options2);

        // Create tables for both contexts
        // EnsureCreated on the first context creates the core tables
        _context1.Database.EnsureCreated();
        // Manually create the second context's table since EnsureCreated won't work after the database exists
        // Note: BaseDbContext uses snake_case naming convention, so we must use snake_case column names
        _context2.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS TestEntities2 (
                id TEXT NOT NULL PRIMARY KEY,
                name TEXT NOT NULL DEFAULT '',
                created_at TEXT NOT NULL,
                updated_at TEXT,
                deleted_at TEXT
            )
        ");

        _unitOfWork = new SharedTransactionUnitOfWork(new List<BaseDbContext> { _context1, _context2 });
    }

    public void Dispose()
    {
        _context1.Dispose();
        _context2.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleContexts_ShouldSaveAllChanges()
    {
        // Arrange
        var entity1 = new TestEntity { Name = "Context1Entity" };
        var entity2 = new TestEntity { Name = "Context2Entity" };

        _context1.TestEntities.Add(entity1);
        _context2.TestEntities.Add(entity2);

        // Act
        int affectedRows = await _unitOfWork.SaveChangesAsync();

        // Assert
        affectedRows.Should().Be(2);

        // Create fresh contexts for verification (old contexts have stale transaction references)
        using var verifyContext1 = new TestDbContext1(new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options);
        using var verifyContext2 = new TestDbContext2(new DbContextOptionsBuilder<TestDbContext2>()
            .UseSqlite(_connection)
            .Options);

        TestEntity? saved1 = await verifyContext1.TestEntities.FirstOrDefaultAsync(e => e.Name == "Context1Entity");
        TestEntity? saved2 = await verifyContext2.TestEntities.FirstOrDefaultAsync(e => e.Name == "Context2Entity");

        saved1.Should().NotBeNull();
        saved2.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoContexts_ShouldReturnZero()
    {
        // Arrange
        var emptyUnitOfWork = new SharedTransactionUnitOfWork(new List<BaseDbContext>());

        // Act
        int affectedRows = await emptyUnitOfWork.SaveChangesAsync();

        // Assert
        affectedRows.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoErrors_ShouldCommitAll()
    {
        // Arrange
        var entity1 = new TestEntity { Name = "Entity1" };
        _context1.TestEntities.Add(entity1);

        var entity2 = new TestEntity { Name = "Entity2" };
        _context2.TestEntities.Add(entity2);

        // Act
        int affectedRows = await _unitOfWork.SaveChangesAsync();

        // Assert
        affectedRows.Should().Be(2);

        // Create fresh contexts for verification
        using var verifyContext1 = new TestDbContext1(new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options);
        using var verifyContext2 = new TestDbContext2(new DbContextOptionsBuilder<TestDbContext2>()
            .UseSqlite(_connection)
            .Options);

        (await verifyContext1.TestEntities.CountAsync()).Should().Be(1);
        (await verifyContext2.TestEntities.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_WithIsolationLevel_ShouldUseSpecifiedLevel()
    {
        // Arrange
        var entity = new TestEntity { Name = "TestEntity" };
        _context1.TestEntities.Add(entity);

        // Act
        int affectedRows = await _unitOfWork.SaveChangesAsync(System.Data.IsolationLevel.ReadCommitted);

        // Assert
        affectedRows.Should().Be(1);

        // Create fresh context for verification
        using var verifyContext = new TestDbContext1(new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options);

        TestEntity? saved = await verifyContext.TestEntities.FirstOrDefaultAsync(e => e.Name == "TestEntity");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldExecuteOperationInTransaction()
    {
        // Arrange
        var entity = new TestEntity { Name = "TransactionEntity" };

        // Act
        int result = await _unitOfWork.ExecuteInTransactionAsync<int>(async contexts =>
        {
            BaseDbContext context1 = contexts.First();
            ((TestDbContext1)context1).TestEntities.Add(entity);
            await context1.SaveChangesAsync();
            return 42;
        });

        // Assert
        result.Should().Be(42);

        // Create fresh context for verification
        using var verifyContext = new TestDbContext1(new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options);

        TestEntity? saved = await verifyContext.TestEntities.FirstOrDefaultAsync(e => e.Name == "TransactionEntity");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithoutReturn_ShouldExecuteOperation()
    {
        // Arrange
        var entity = new TestEntity { Name = "VoidTransactionEntity" };

        // Act
        await _unitOfWork.ExecuteInTransactionAsync(async contexts =>
        {
            BaseDbContext context1 = contexts.First();
            ((TestDbContext1)context1).TestEntities.Add(entity);
            await context1.SaveChangesAsync();
        });

        // Assert
        // Create fresh context for verification
        using var verifyContext = new TestDbContext1(new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options);

        TestEntity? saved = await verifyContext.TestEntities.FirstOrDefaultAsync(e => e.Name == "VoidTransactionEntity");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WhenExceptionThrown_ShouldRollback()
    {
        // Arrange
        var entity = new TestEntity { Name = "RollbackEntity" };

        // Act
        Func<Task> act = async () => await _unitOfWork.ExecuteInTransactionAsync<int>(async contexts =>
        {
            BaseDbContext context1 = contexts.First();
            ((TestDbContext1)context1).TestEntities.Add(entity);
            await context1.SaveChangesAsync();
            throw new InvalidOperationException("Test exception");
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify rollback - entity should not be saved
        using var verifyContext = new TestDbContext1(new DbContextOptionsBuilder<TestDbContext1>()
            .UseSqlite(_connection)
            .Options);

        TestEntity? saved = await verifyContext.TestEntities.FirstOrDefaultAsync(e => e.Name == "RollbackEntity");
        saved.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_WithNoContexts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var emptyUnitOfWork = new SharedTransactionUnitOfWork(new List<BaseDbContext>());

        // Act
        Func<Task> act = async () => await emptyUnitOfWork.ExecuteInTransactionAsync<int>(async contexts => await Task.FromResult(42));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No DbContexts available*");
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_VoidWithNoContexts_ShouldNotThrow()
    {
        // Arrange
        var emptyUnitOfWork = new SharedTransactionUnitOfWork(new List<BaseDbContext>());

        // Act
        Func<Task> act = async () => await emptyUnitOfWork.ExecuteInTransactionAsync(async contexts => await Task.CompletedTask);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
