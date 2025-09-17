# MasLazu.AspNet.Framework.EfCore

EF Core infrastructure layer for MasLazu.AspNet.Framework - Contains repository implementations, database contexts, and data access patterns.

## Features

- Repository pattern with read/write separation
- Generic repository implementations
- Unit of Work pattern
- Soft delete support
- Dynamic query building
- Pagination support

## Installation

```bash
dotnet add package MasLazu.AspNet.Framework.EfCore
```

## Usage

```csharp
using MasLazu.AspNet.Framework.EfCore.Data;

public class MyDbContext : BaseDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

    public DbSet<MyEntity> MyEntities { get; set; }
}
```

## Dependencies

- `MasLazu.AspNet.Framework.Domain`
- `MasLazu.AspNet.Framework.Application`
- `Microsoft.EntityFrameworkCore`
- `EFCore.NamingConventions`

## Documentation

For complete documentation, see the [main README](https://github.com/MasLazu/MasLazu.AspNet.Framework#readme).

## License

MIT
