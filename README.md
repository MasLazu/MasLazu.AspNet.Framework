# MasLazu.AspNet.Framework

A modern, clean architecture ASP.NET Core framework built with .NET 9, designed for building scalable and maintainable web APIs.

## 🚀 Features

- **Clean Architecture**: Strict separation of concerns with Domain, Application, Infrastructure, and Presentation layers
- **FastEndpoints Integration**: High-performance minimal APIs with automatic OpenAPI documentation
- **Generic CRUD Operations**: Reusable CRUD services with built-in validation and mapping
- **Entity Framework Core**: Full ORM support with PostgreSQL provider
- **Soft Delete Support**: Automatic soft deletion with audit trails
- **Pagination**: Both offset-based and cursor-based pagination with comprehensive validation
- **Advanced Validation**: FluentValidation with field existence validation, operator compatibility, and custom error responses
- **Dependency Injection**: Built-in DI container with service registration
- **User Context**: Built-in user-based authorization and auditing
- **Modern .NET**: Leverages .NET 9 features including nullable types and records
- **Property Mapping**: Dynamic field validation for ordering and filtering operations

## 📁 Project Structure

```
MasLazu.AspNet.Framework.sln
src/
├── MasLazu.AspNet.Framework.Domain/
│   └── Entities/
│       └── BaseEntity.cs
├── MasLazu.AspNet.Framework.Application/
│   ├── Interfaces/
│   │   ├── ICursorPaginationValidator.cs
│   │   ├── ICrudService.cs
│   │   ├── IEntityPropertyMap.cs
│   │   ├── IPaginationValidator.cs
│   │   ├── IRepository.cs
│   │   └── IReadRepository.cs
│   ├── Models/
│   │   ├── BaseDto.cs
│   │   ├── CursorPaginationRequest.cs
│   │   ├── CursorPaginatedResult.cs
│   │   ├── PaginationRequest.cs
│   │   └── PaginatedResult.cs
│   ├── Services/
│   │   └── CrudService.cs
│   ├── Validators/
│   │   ├── CursorPaginationRequestValidator.cs
│   │   ├── PaginationRequestValidator.cs
│   └── Utils/
├── MasLazu.AspNet.Framework.EfCore/
│   ├── Configurations/
│   ├── Data/
│   │   └── BaseDbContext.cs
│   ├── Extensions/
│   ├── Repositories/
│   │   ├── Repository.cs
│   │   └── ReadRepository.cs
│   └── Services/
├── MasLazu.AspNet.Framework.EfCore.Postgresql/
│   ├── Data/
│   └── Extensions/
└── MasLazu.AspNet.Framework.Endpoint/
    ├── EndpointGroups/
    │   ├── ApiEndpointGroup.cs
    │   └── V1EndpointGroup.cs
    ├── Endpoints/
    │   ├── BaseEndpoint.cs
    │   ├── BaseEndpointWithoutRequest.cs
    │   └── BaseEndpointWithoutResponse.cs
    ├── Extensions/
    ├── Middlewares/
    ├── Models/
    │   ├── SuccessResponse.cs
    │   └── ErrorResponse.cs
    └── Utils/
```

## 🏗️ Architecture

### Domain Layer

- Contains core business entities and domain logic
- `BaseEntity` provides common properties: `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`
- No external dependencies

### Application Layer

- Business logic and use cases
- Generic `CrudService` for common operations
- DTOs and request/response models
- **Advanced validation** with field existence checking and operator compatibility
- **Dual pagination support** with comprehensive request validation
- **Property mapping** for dynamic field validation and ordering

### Infrastructure Layer

- EF Core implementations
- Repository pattern with read/write separation
- Database contexts and configurations
- External service integrations

### Presentation Layer

- FastEndpoints-based API endpoints
- Standardized response models
- Endpoint groups for versioning
- Middleware and cross-cutting concerns

## 🔍 Advanced Features

### Pagination & Validation

The framework provides comprehensive pagination and validation capabilities:

#### Offset-Based Pagination

```csharp
// Request
{
  "page": 1,
  "pageSize": 20,
  "filters": [
    { "field": "name", "operator": "contains", "value": "test" }
  ],
  "orderBy": [
    { "field": "createdAt", "desc": true }
  ]
}

// Response
{
  "totalCount": 150,
  "pageSize": 20,
  "page": 1,
  "items": [...]
}
```

#### Cursor-Based Pagination

```csharp
// Request
{
  "limit": 20,
  "cursor": "eyJpZCI6IjEyMyJ9", // Optional
  "filters": [
    { "field": "status", "operator": "=", "value": "active" }
  ],
  "orderBy": [
    { "field": "id", "desc": false }
  ]
}

// Response
{
  "items": [...],
  "nextCursor": "eyJpZCI6IjE0MyJ9"
}
```

#### Validation Features

- **Field Existence**: Validates that filter/order fields exist and are available for operations
- **Operator Compatibility**: Ensures operators are valid for field types
- **Type Safety**: Validates data types match field types
- **Custom Error Messages**: Clear, actionable error responses

#### Supported Operators by Type

- **String**: `=`, `!=`, `contains`, `startswith`, `endswith`
- **Numeric/DateTime**: `=`, `!=`, `>`, `<`, `>=`, `<=`
- **Boolean**: `=`, `!=`
- **Enum/GUID**: `=`, `!=`

## 📦 Dependencies

### Core Dependencies

- **.NET 9.0**
- **FastEndpoints** - Minimal API framework
- **Entity Framework Core** - ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL** - PostgreSQL provider
- **FluentValidation** - Validation framework
- **Mapster** - Object mapping
- **Microsoft.Extensions.DependencyInjection** - DI container

### Development Dependencies

- **EFCore.NamingConventions** - Snake case naming
- **Microsoft.EntityFrameworkCore.Relational** - Relational features
- **Microsoft.Extensions.Configuration** - Configuration

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL database
- Git

### Installation

1. Clone the repository:

```bash
git clone <repository-url>
cd MasLazu.AspNet.Framework
```

2. Restore packages:

```bash
dotnet restore
```

3. Configure your database connection in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=your_db;Username=your_user;Password=your_password"
  }
}
```

4. Run database migrations:

```bash
dotnet ef database update
```

5. Run the application:

```bash
dotnet run
```

## 💡 Usage Examples

### Creating a New Entity

1. Define your domain entity:

```csharp
using MasLazu.AspNet.Framework.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

2. Create DTOs:

```csharp
using MasLazu.AspNet.Framework.Application.Models;

public record ProductDto(Guid Id, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt, string Name, decimal Price, string Description) : BaseDto(Id, CreatedAt, UpdatedAt);

public record CreateProductRequest(string Name, decimal Price, string Description);

public record UpdateProductRequest(Guid Id, string Name, decimal Price, string Description) : BaseUpdateRequest(Id);
```

3. Implement the service:

```csharp
using MasLazu.AspNet.Framework.Application.Services;

public class ProductService : CrudService<Product, ProductDto, CreateProductRequest, UpdateProductRequest>
{
    public ProductService(
        IRepository<Product> repository,
        IReadRepository<Product> readRepository,
        IUnitOfWork unitOfWork,
        IEntityPropertyMap<Product> propertyMap,
        IPaginationValidator<Product> paginationValidator,
        ICursorPaginationValidator<Product> cursorPaginationValidator,
        IValidator<CreateProductRequest>? createValidator = null,
        IValidator<UpdateProductRequest>? updateValidator = null)
        : base(repository, readRepository, unitOfWork, propertyMap, paginationValidator, cursorPaginationValidator, createValidator, updateValidator)
    {
    }

    // Use cursor pagination for large datasets
    public async Task<CursorPaginatedResult<ProductDto>> GetProductsCursorAsync(CursorPaginationRequest request)
    {
        return await GetCursorPaginatedAsync(Guid.Empty, request);
    }
}
```

4. Create the endpoint:

```csharp
using MasLazu.AspNet.Framework.Endpoint.Endpoints;
using MasLazu.AspNet.Framework.Endpoint.EndpointGroups;

public class GetProductsEndpoint : BaseEndpointWithoutRequest<List<ProductDto>>
{
    public override void ConfigureEndpoint()
    {
        Get("/products");
        Group<V1EndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var service = Resolve<ProductService>();
        var products = await service.GetAllAsync(Guid.Empty, ct);
        await SendOkResponseAsync(products.ToList());
    }
}
```

### Database Context Setup

```csharp
using MasLazu.AspNet.Framework.EfCore.Data;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : BaseDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Your entity configurations here
    }
}
```

## 🔧 Configuration

### Dependency Injection Setup

```csharp
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Extensions;
using MasLazu.AspNet.Framework.EfCore.Repositories;
using MasLazu.AspNet.Framework.EfCore.Data;

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<,>));
builder.Services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<,>));
builder.Services.AddScoped<IUnitOfWork, SharedTransactionUnitOfWork>();
builder.Services.AddScoped<AppDbContext>();

// Add framework validators
builder.Services.AddFrameworkApplicationValidators();
```

### FastEndpoints Configuration

```csharp
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseFastEndpoints();
app.Run();
```

## 📚 Key Concepts

### Soft Deletes

All entities inherit from `BaseEntity` which includes a `DeletedAt` field. The framework automatically filters out soft-deleted records in queries.

### User Context

All service methods include a `userId` parameter for authorization and auditing purposes.

### Generic CRUD

The `CrudService` provides common CRUD operations that can be extended or overridden as needed.

### Pagination

Supports both traditional **offset-based pagination** and modern **cursor-based pagination**:

- **Offset Pagination**: Traditional page-based navigation with total count
- **Cursor Pagination**: Token-based navigation for better performance with large datasets
- **Field Validation**: Validates that sort and filter fields exist and are available for operations
- **Operator Validation**: Ensures filter operators are compatible with field types
- **Type Safety**: Runtime validation of field types and values

### Validation

Integrated FluentValidation with advanced features:

- **Field Existence**: Validates fields exist in the entity property map
- **Operator Compatibility**: Checks operators against field types
- **Custom Error Messages**: Clear, actionable validation errors
- **Automatic DI**: Validators automatically registered via extension methods

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙋 Support

For questions and support, please open an issue on GitHub.

---

Built with ❤️ using .NET 9 and modern architectural patterns.
