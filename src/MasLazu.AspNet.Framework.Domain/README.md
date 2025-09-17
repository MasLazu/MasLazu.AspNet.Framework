# MasLazu.AspNet.Framework.Domain

Core domain layer for MasLazu.AspNet.Framework - Contains base entities, domain logic, and fundamental building blocks.

## Features

- `BaseEntity` with soft delete support and audit trails
- Common domain patterns and interfaces
- Foundation for the entire framework

## Installation

```bash
dotnet add package MasLazu.AspNet.Framework.Domain
```

## Usage

```csharp
using MasLazu.AspNet.Framework.Domain.Entities;

public class MyEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    // Your properties here
}
```

## Documentation

For complete documentation, see the [main README](https://github.com/MasLazu/MasLazu.AspNet.Framework#readme).

## License

MIT
