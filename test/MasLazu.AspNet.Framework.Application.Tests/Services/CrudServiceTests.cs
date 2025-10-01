using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using MasLazu.AspNet.Framework.Application.Exceptions;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Services;
using MasLazu.AspNet.Framework.Domain.Entities;
using Xunit;

namespace MasLazu.AspNet.Framework.Application.Tests.Services;

public class CrudServiceTests
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public record TestDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, string Name, int Age) : BaseDto(Id, CreatedAt, UpdatedAt);
    public record CreateTestRequest(string Name, int Age);
    public record UpdateTestRequest(Guid Id, string Name, int Age) : BaseUpdateRequest(Id);

    public class TestCrudService : CrudService<TestEntity, TestDto, CreateTestRequest, UpdateTestRequest>
    {
        public TestCrudService(
            IRepository<TestEntity> repository,
            IReadRepository<TestEntity> readRepository,
            IUnitOfWork unitOfWork,
            IEntityPropertyMap<TestEntity> propertyMap,
            IPaginationValidator<TestEntity> paginationValidator,
            ICursorPaginationValidator<TestEntity> cursorPaginationValidator,
            IValidator<CreateTestRequest>? createValidator = null,
            IValidator<UpdateTestRequest>? updateValidator = null)
            : base(repository, readRepository, unitOfWork, propertyMap, paginationValidator, cursorPaginationValidator, createValidator, updateValidator)
        {
        }
    }

    private readonly Mock<IRepository<TestEntity>> _repositoryMock;
    private readonly Mock<IReadRepository<TestEntity>> _readRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEntityPropertyMap<TestEntity>> _propertyMapMock;
    private readonly Mock<IPaginationValidator<TestEntity>> _paginationValidatorMock;
    private readonly Mock<ICursorPaginationValidator<TestEntity>> _cursorPaginationValidatorMock;
    private readonly Mock<IValidator<CreateTestRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateTestRequest>> _updateValidatorMock;
    private readonly TestCrudService _service;

    public CrudServiceTests()
    {
        _repositoryMock = new Mock<IRepository<TestEntity>>();
        _readRepositoryMock = new Mock<IReadRepository<TestEntity>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _propertyMapMock = new Mock<IEntityPropertyMap<TestEntity>>();
        _paginationValidatorMock = new Mock<IPaginationValidator<TestEntity>>();
        _cursorPaginationValidatorMock = new Mock<ICursorPaginationValidator<TestEntity>>();
        _createValidatorMock = new Mock<IValidator<CreateTestRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTestRequest>>();

        _service = new TestCrudService(
            _repositoryMock.Object,
            _readRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _propertyMapMock.Object,
            _paginationValidatorMock.Object,
            _cursorPaginationValidatorMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object
        );
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenEntityExists_ShouldReturnDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "Test", Age = 25 };
        _readRepositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync(entity);

        // Act
        TestDto? result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("Test");
        result.Age.Should().Be(25);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEntityDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _readRepositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync((TestEntity?)null);

        // Act
        TestDto? result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDtos()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Test1", Age = 25 },
            new() { Id = Guid.NewGuid(), Name = "Test2", Age = 30 }
        };
        _readRepositoryMock.Setup(r => r.GetAllAsync(CancellationToken.None))
            .ReturnsAsync(entities);

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Test1");
        result[1].Name.Should().Be("Test2");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoEntities_ShouldReturnEmptyCollection()
    {
        // Arrange
        _readRepositoryMock.Setup(r => r.GetAllAsync(CancellationToken.None))
            .ReturnsAsync(new List<TestEntity>());

        // Act
        IEnumerable<TestDto> result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateAndReturnDto()
    {
        // Arrange
        var request = new CreateTestRequest("Test", 25);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test", Age = 25 };

        _createValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<TestEntity>(), default))
            .ReturnsAsync(entity);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        TestDto result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
        result.Age.Should().Be(25);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_AndSaveChangesFalse_ShouldNotSaveChanges()
    {
        // Arrange
        var request = new CreateTestRequest("Test", 25);
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Test", Age = 25 };

        _createValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<TestEntity>(), default))
            .ReturnsAsync(entity);

        // Act
        TestDto result = await _service.CreateAsync(request, saveChanges: false);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var request = new CreateTestRequest("", -1);
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required")
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region CreateIfNotExistAsync Tests

    [Fact]
    public async Task CreateIfNotExistAsync_WhenEntityDoesNotExist_ShouldCreate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CreateTestRequest("Test", 25);
        var entity = new TestEntity { Id = id, Name = "Test", Age = 25 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync((TestEntity?)null);
        _createValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<TestEntity>(), default))
            .ReturnsAsync(entity);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        TestDto result = await _service.CreateIfNotExistAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TestEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateIfNotExistAsync_WhenEntityExists_ShouldReturnExisting()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new CreateTestRequest("Test", 25);
        var existingEntity = new TestEntity { Id = id, Name = "Existing", Age = 30 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync(existingEntity);
        _createValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        TestDto result = await _service.CreateIfNotExistAsync(id, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Existing");
        result.Age.Should().Be(30);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TestEntity>(), default), Times.Never);
    }

    #endregion

    #region CreateRangeAsync Tests

    [Fact]
    public async Task CreateRangeAsync_WithValidRequests_ShouldCreateAll()
    {
        // Arrange
        var requests = new List<CreateTestRequest>
        {
            new("Test1", 25),
            new("Test2", 30)
        };
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Test1", Age = 25 },
            new() { Id = Guid.NewGuid(), Name = "Test2", Age = 30 }
        };

        _createValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTestRequest>(), default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<TestEntity>>(), default))
            .ReturnsAsync(entities);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(2);

        // Act
        var result = (await _service.CreateRangeAsync(requests)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Test1");
        result[1].Name.Should().Be("Test2");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateTestRequest(id, "Updated", 35);
        var existingEntity = new TestEntity { Id = id, Name = "Original", Age = 25 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync(existingEntity);
        _updateValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>(), default))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        TestDto result = await _service.UpdateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated");
        result.Age.Should().Be(35);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateTestRequest(id, "Updated", 35);

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync((TestEntity?)null);

        // Act
        Func<Task> act = async () => await _service.UpdateAsync(request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithSaveChangesFalse_ShouldNotSaveChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateTestRequest(id, "Updated", 35);
        var existingEntity = new TestEntity { Id = id, Name = "Original", Age = 25 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync(existingEntity);
        _updateValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TestEntity>(), default))
            .Returns(Task.CompletedTask);

        // Act
        TestDto result = await _service.UpdateAsync(request, saveChanges: false);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    #endregion

    #region UpdateRangeAsync Tests

    [Fact]
    public async Task UpdateRangeAsync_WithValidRequests_ShouldUpdateAll()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var requests = new List<UpdateTestRequest>
        {
            new(id1, "Updated1", 35),
            new(id2, "Updated2", 40)
        };
        var existingEntities = new List<TestEntity>
        {
            new() { Id = id1, Name = "Original1", Age = 25 },
            new() { Id = id2, Name = "Original2", Age = 30 }
        };

        _repositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(existingEntities);
        _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTestRequest>(), default))
            .ReturnsAsync(new ValidationResult());
        _repositoryMock.Setup(r => r.UpdateRangeAsync(It.IsAny<List<TestEntity>>(), default))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(2);

        // Act
        var result = (await _service.UpdateRangeAsync(requests)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Updated1");
        result[1].Name.Should().Be("Updated2");
    }

    [Fact]
    public async Task UpdateRangeAsync_WhenSomeEntitiesNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var requests = new List<UpdateTestRequest>
        {
            new(id1, "Updated1", 35),
            new(id2, "Updated2", 40)
        };
        var existingEntities = new List<TestEntity>
        {
            new() { Id = id1, Name = "Original1", Age = 25 }
        };

        _repositoryMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), default))
            .ReturnsAsync(existingEntities);
        _updateValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTestRequest>(), default))
            .ReturnsAsync(new ValidationResult());

        // Act
        Func<Task> act = async () => await _service.UpdateRangeAsync(requests);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenEntityExists_ShouldDelete()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "Test", Age = 25 };

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.DeleteAsync(entity, default))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(id);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(entity, default), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenEntityNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(r => r.GetByIdAsync(id, CancellationToken.None))
            .ReturnsAsync((TestEntity?)null);

        // Act
        Func<Task> act = async () => await _service.DeleteAsync(id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteRangeAsync Tests

    [Fact]
    public async Task DeleteRangeAsync_WhenAllEntitiesExist_ShouldDeleteAll()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var entities = new List<TestEntity>
        {
            new() { Id = ids[0], Name = "Test1", Age = 25 },
            new() { Id = ids[1], Name = "Test2", Age = 30 }
        };

        _repositoryMock.Setup(r => r.GetByIdsAsync(ids, default))
            .ReturnsAsync(entities);
        _repositoryMock.Setup(r => r.DeleteRangeAsync(entities, default))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default))
            .ReturnsAsync(2);

        // Act
        await _service.DeleteRangeAsync(ids);

        // Assert
        _repositoryMock.Verify(r => r.DeleteRangeAsync(entities, default), Times.Once);
    }

    [Fact]
    public async Task DeleteRangeAsync_WhenSomeEntitiesNotFound_ShouldThrowBadRequestException()
    {
        // Arrange
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var entities = new List<TestEntity>
        {
            new() { Id = ids[0], Name = "Test1", Age = 25 }
        };

        _repositoryMock.Setup(r => r.GetByIdsAsync(ids, default))
            .ReturnsAsync(entities);

        // Act
        Func<Task> act = async () => await _service.DeleteRangeAsync(ids);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WhenEntityExists_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _readRepositoryMock.Setup(r => r.ExistsAsync(id, default))
            .ReturnsAsync(true);

        // Act
        bool result = await _service.ExistsAsync(id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _readRepositoryMock.Setup(r => r.ExistsAsync(id, default))
            .ReturnsAsync(false);

        // Act
        bool result = await _service.ExistsAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_ShouldReturnCount()
    {
        // Arrange
        _readRepositoryMock.Setup(r => r.CountAsync(default))
            .ReturnsAsync(42);

        // Act
        int result = await _service.CountAsync();

        // Assert
        result.Should().Be(42);
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_ShouldReturnPaginatedResult()
    {
        // Arrange
        var request = new PaginationRequest
        {
            Page = 1,
            PageSize = 10
        };
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Test1", Age = 25 },
            new() { Id = Guid.NewGuid(), Name = "Test2", Age = 30 }
        };

        _paginationValidatorMock.Setup(v => v.ValidateAsync(It.IsAny<PaginationRequest<TestEntity>>(), default))
            .ReturnsAsync(new ValidationResult());
        _readRepositoryMock.Setup(r => r.GetPaginatedAsync(request, default))
            .ReturnsAsync((entities, 100));

        // Act
        PaginatedResult<TestDto> result = await _service.GetPaginatedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(100);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    #endregion

    #region GetCursorPaginatedAsync Tests

    [Fact]
    public async Task GetCursorPaginatedAsync_ShouldReturnCursorPaginatedResult()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10
        };
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Test1", Age = 25 },
            new() { Id = Guid.NewGuid(), Name = "Test2", Age = 30 }
        };
        var nextCursor = Guid.NewGuid();

        _cursorPaginationValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _readRepositoryMock.Setup(r => r.GetCursorPaginatedAsync(request, default))
            .ReturnsAsync((entities, nextCursor));

        // Act
        CursorPaginatedResult<TestDto> result = await _service.GetCursorPaginatedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.NextCursor.Should().Be(nextCursor.ToString());
    }

    [Fact]
    public async Task GetCursorPaginatedAsync_WithNoNextCursor_ShouldReturnNullNextCursor()
    {
        // Arrange
        var request = new CursorPaginationRequest
        {
            Limit = 10
        };
        var entities = new List<TestEntity>
        {
            new() { Id = Guid.NewGuid(), Name = "Test1", Age = 25 }
        };

        _cursorPaginationValidatorMock.Setup(v => v.ValidateAsync(request, default))
            .ReturnsAsync(new ValidationResult());
        _readRepositoryMock.Setup(r => r.GetCursorPaginatedAsync(request, default))
            .ReturnsAsync((entities, (Guid?)null));

        // Act
        CursorPaginatedResult<TestDto> result = await _service.GetCursorPaginatedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.NextCursor.Should().BeNull();
    }

    #endregion
}
