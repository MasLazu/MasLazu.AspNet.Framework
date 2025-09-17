# MasLazu.AspNet.Framework.EfCore.Postgresql

PostgreSQL provider for MasLazu.AspNet.Framework.EfCore - Adds PostgreSQL support to the EF Core infrastructure layer.

## Features

- PostgreSQL database provider
- Snake case naming conventions
- Optimized for PostgreSQL performance
- Full integration with the framework's EF Core layer

## Installation

```bash
dotnet add package MasLazu.AspNet.Framework.EfCore.Postgresql
```

## Usage

```csharp
using MasLazu.AspNet.Framework.EfCore.Data;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
}
```

## Dependencies

- `MasLazu.AspNet.Framework.EfCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

## Database Setup

Make sure you have PostgreSQL installed and running. Your connection string should look like:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mydb;Username=myuser;Password=mypass"
  }
}
```

## Documentation

For complete documentation, see the [main README](https://github.com/MasLazu/MasLazu.AspNet.Framework#readme).

## License

MIT
