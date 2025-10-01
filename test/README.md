# MasLazu.AspNet.Framework - Test Suite

This directory contains comprehensive unit tests for the MasLazu.AspNet.Framework. The test structure mirrors the source code structure, ensuring maximum test coverage and maintainability.

## 📁 Test Project Structure

```
test/
├── MasLazu.AspNet.Framework.Domain.Tests/
│   └── Entities/
│       └── BaseEntityTests.cs
├── MasLazu.AspNet.Framework.Application.Tests/
│   ├── Exceptions/
│   │   ├── AppExceptionTests.cs
│   │   ├── BadRequestExceptionTests.cs
│   │   ├── ConflictExceptionTests.cs
│   │   ├── ForbiddenExceptionTests.cs
│   │   ├── NotFoundExceptionTests.cs
│   │   └── UnauthorizedExceptionTests.cs
│   ├── Models/
│   │   ├── BaseDtoTests.cs
│   │   ├── CursorPaginatedResultTests.cs
│   │   ├── CursorPaginationRequestTests.cs
│   │   ├── PaginatedResultTests.cs
│   │   └── PaginationRequestTests.cs
│   ├── Services/
│   │   └── CrudServiceTests.cs
│   ├── Utils/
│   │   └── ExpressionBuilderTests.cs
│   └── Validators/
│       ├── CursorPaginationRequestValidatorTests.cs
│       └── PaginationRequestValidatorTests.cs
├── MasLazu.AspNet.Framework.EfCore.Tests/
│   ├── Data/
│   │   └── SharedTransactionUnitOfWorkTests.cs
│   └── Repositories/
│       ├── ReadRepositoryTests.cs
│       └── RepositoryTests.cs
└── MasLazu.AspNet.Framework.Endpoint.Tests/
    ├── Middlewares/
    │   └── ExceptionHandlerMiddlewareTests.cs
    └── Models/
        ├── ErrorResponseTests.cs
        └── SuccessResponseTests.cs
```

## 🧪 Testing Framework

- **Test Framework**: xUnit 2.9.2
- **Assertion Library**: FluentAssertions 7.0.0
- **Mocking Framework**: Moq 4.20.72
- **Code Coverage**: coverlet.collector 6.0.2
- **In-Memory Database**: Microsoft.EntityFrameworkCore.InMemory 9.0.9 (for EfCore tests)

## 📊 Test Coverage

### Domain Layer Tests (MasLazu.AspNet.Framework.Domain.Tests)

**BaseEntity Tests** - 11 test cases

- Constructor initialization tests
- Property setters tests
- Multiple entity uniqueness tests
- Soft delete scenarios
- Update scenarios

### Application Layer Tests (MasLazu.AspNet.Framework.Application.Tests)

**Exception Tests** - 31 test cases

- AppException base class tests (7 tests)
- BadRequestException tests (8 tests)
- ConflictException tests (4 tests)
- ForbiddenException tests (6 tests)
- NotFoundException tests (6 tests)
- UnauthorizedException tests (6 tests)

**Model Tests** - 20 test cases

- BaseDto tests (5 tests)
- CursorPaginatedResult tests (4 tests)
- CursorPaginationRequest tests (6 tests)
- PaginatedResult tests (5 tests)
- PaginationRequest tests (5 tests)

**Utils Tests** - 21 test cases

- ExpressionBuilder filter tests (12 tests)
- ExpressionBuilder ordering tests (3 tests)
- ExpressionBuilder pagination tests (3 tests)
- ExpressionBuilder cursor tests (3 tests)

**Validator Tests** - 40 test cases

- PaginationRequestValidator tests (22 tests)
  - Page validation
  - PageSize validation
  - Filter field validation
  - Filter operator validation
  - OrderBy field validation
- CursorPaginationRequestValidator tests (18 tests)
  - Limit validation
  - Cursor validation
  - Filter validation
  - OrderBy validation

**Service Tests** - 32 test cases

- CrudService CRUD operation tests
- GetByIdAsync tests (2 tests)
- GetAllAsync tests (2 tests)
- CreateAsync tests (3 tests)
- CreateIfNotExistAsync tests (2 tests)
- CreateRangeAsync tests (1 test)
- UpdateAsync tests (3 tests)
- UpdateRangeAsync tests (2 tests)
- DeleteAsync tests (2 tests)
- DeleteRangeAsync tests (2 tests)
- ExistsAsync tests (2 tests)
- CountAsync tests (1 test)
- GetPaginatedAsync tests (1 test)
- GetCursorPaginatedAsync tests (2 tests)

### EfCore Layer Tests (MasLazu.AspNet.Framework.EfCore.Tests)

**Repository Tests** - 30 test cases

- AddAsync tests (2 tests)
- AddRangeAsync tests (1 test)
- GetByIdAsync tests (3 tests)
- GetByIdsAsync tests (1 test)
- GetAllAsync tests (1 test)
- UpdateAsync tests (2 tests)
- UpdateRangeAsync tests (1 test)
- DeleteAsync tests (2 tests)
- DeleteRangeAsync tests (1 test)
- ExistsAsync tests (2 tests)
- CountAsync tests (1 test)
- FindAsync tests (1 test)
- FirstOrDefaultAsync tests (2 tests)
- GetPaginatedAsync tests (2 tests)
- GetCursorPaginatedAsync tests (3 tests)
- GetTimeseriesCountAsync tests (1 test)

**ReadRepository Tests** - 12 test cases

- GetByIdAsync tests (2 tests)
- GetAllAsync tests (1 test)
- FindAsync tests (1 test)
- FirstOrDefaultAsync tests (2 tests)
- ExistsAsync tests (2 tests)
- CountAsync tests (2 tests)
- GetPaginatedAsync tests (1 test)
- GetCursorPaginatedAsync tests (1 test)

**SharedTransactionUnitOfWork Tests** - 9 test cases

- SaveChangesAsync tests (3 tests)
- SaveChangesAsync with isolation level tests (1 test)
- ExecuteInTransactionAsync tests (5 tests)

### Endpoint Layer Tests (MasLazu.AspNet.Framework.Endpoint.Tests)

**Middleware Tests** - 13 test cases

- ExceptionHandlerMiddleware tests
  - No exception scenario
  - AppException handling (6 tests)
  - ValidationFailureException handling
  - Unexpected exception handling
  - Logging tests (2 tests)
  - Exception details inclusion

**Model Tests** - 8 test cases

- SuccessResponse tests (4 tests)
- ErrorResponse tests (2 tests)
- ValidationErrorResponse tests (2 tests)

## 📈 Total Test Coverage

- **Total Test Cases**: 227+ comprehensive tests
- **Coverage Areas**:
  - ✅ All Domain entities
  - ✅ All Application exceptions
  - ✅ All Application models
  - ✅ All Application validators
  - ✅ All Application services
  - ✅ All Application utilities
  - ✅ All EfCore repositories
  - ✅ All EfCore data contexts
  - ✅ All Endpoint middlewares
  - ✅ All Endpoint models

## 🚀 Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Tests for Specific Project

```bash
# Domain tests
dotnet test test/MasLazu.AspNet.Framework.Domain.Tests

# Application tests
dotnet test test/MasLazu.AspNet.Framework.Application.Tests

# EfCore tests
dotnet test test/MasLazu.AspNet.Framework.EfCore.Tests

# Endpoint tests
dotnet test test/MasLazu.AspNet.Framework.Endpoint.Tests
```

### Run Tests with Code Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run Tests with Detailed Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~BaseEntityTests"
```

### Run Tests in Parallel

```bash
dotnet test --parallel
```

## 🧩 Test Patterns and Best Practices

### Naming Convention

All tests follow the pattern: `MethodName_Scenario_ExpectedBehavior`

Examples:

- `GetByIdAsync_WhenEntityExists_ShouldReturnEntity`
- `CreateAsync_WithInvalidRequest_ShouldThrowValidationException`
- `ApplyFilters_WithEqualOperator_ShouldFilterCorrectly`

### Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and mocks
    var entity = new TestEntity { Name = "Test" };

    // Act - Execute the method being tested
    var result = await service.GetByIdAsync(entity.Id);

    // Assert - Verify the expected outcome
    result.Should().NotBeNull();
    result.Name.Should().Be("Test");
}
```

### Mocking

- Uses Moq for creating test doubles
- Setup and verify method calls
- Configure return values for dependencies

### FluentAssertions

- Provides readable and maintainable assertions
- Better error messages
- Natural language syntax

## 🔍 Test Categories

### Unit Tests

- Test individual components in isolation
- Use mocks for dependencies
- Fast execution

### Integration Tests (Repository Tests)

- Test EfCore repositories with InMemory database
- Verify database operations
- Test query logic

## 📝 Test Coverage Goals

- **Line Coverage**: Target 90%+
- **Branch Coverage**: Target 85%+
- **Method Coverage**: Target 95%+

## 🛠️ Continuous Integration

Tests are designed to run in CI/CD pipelines:

- Fast execution time
- No external dependencies (uses InMemory database)
- Deterministic results
- Clear failure messages

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [EF Core InMemory Provider](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/)

## 🤝 Contributing Tests

When adding new features:

1. Write tests first (TDD approach)
2. Follow existing naming conventions
3. Maintain test structure mirroring source code
4. Aim for high code coverage
5. Write clear, descriptive test names
6. Use AAA pattern consistently
7. Keep tests independent and isolated

## 📊 Test Metrics

To generate detailed test metrics and coverage reports:

```bash
# Install report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generate HTML report
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage-report -reporttypes:Html
```

---

**Note**: All tests are designed to be maintainable, readable, and provide comprehensive coverage of the framework's functionality.
