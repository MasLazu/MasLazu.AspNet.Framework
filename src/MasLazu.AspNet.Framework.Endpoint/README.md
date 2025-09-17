# MasLazu.AspNet.Framework.Endpoint

Presentation layer for MasLazu.AspNet.Framework - Contains FastEndpoints implementations, response models, and API endpoint patterns.

## Features

- FastEndpoints integration for high-performance APIs
- Standardized response models (`SuccessResponse<T>`, `ErrorResponse`)
- Base endpoint classes for different scenarios
- Endpoint groups for API versioning
- Built-in validation and error handling

## Installation

```bash
dotnet add package MasLazu.AspNet.Framework.Endpoint
```

## Usage

```csharp
using MasLazu.AspNet.Framework.Endpoint.Endpoints;

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

## Dependencies

- `MasLazu.AspNet.Framework.Application`
- `FastEndpoints`
- `Microsoft.AspNetCore.App`

## Program.cs Setup

```csharp
using FastEndpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints();

var app = builder.Build();
app.UseFastEndpoints();
app.Run();
```

## Documentation

For complete documentation, see the [main README](https://github.com/MasLazu/MasLazu.AspNet.Framework#readme).

## License

MIT
