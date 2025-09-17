# MasLazu.AspNet.Framework.Application

Application layer for MasLazu.AspNet.Framework - Contains business logic, services, DTOs, and interfaces with generic CRUD operations.

## Features

- Generic `CrudService` for common operations
- Built-in validation with FluentValidation
- Object mapping with Mapster
- Pagination support (offset and cursor-based)
- User context for authorization

## Installation

```bash
dotnet add package MasLazu.AspNet.Framework.Application
```

## Usage

```csharp
using MasLazu.AspNet.Framework.Application.Services;

public class MyService : CrudService<MyEntity, MyDto, CreateRequest, UpdateRequest>
{
    // Inherits full CRUD functionality with validation and mapping
}
```

## Dependencies

- `MasLazu.AspNet.Framework.Domain`
- `FluentValidation`
- `Mapster`
- `Microsoft.Extensions.DependencyInjection`

## Documentation

For complete documentation, see the [main README](https://github.com/MasLazu/MasLazu.AspNet.Framework#readme).

## License

MIT
