# CifraId

ASP.NET Core and System.Text.Json support for **reversible obfuscated ID serialization** and model binding.

CifraId hides raw integer database IDs from API consumers by encoding them as numeric-looking strings. Mark your DTO properties with `[CifraId]` and the library handles JSON serialization, deserialization, and query-string model binding automatically.

| Package | Purpose |
|---|---|
| `CifraId.Core` | Core encoding, services, `[CifraId]`, and `System.Text.Json` converter support |
| `CifraId.AspNetCore` | ASP.NET Core MVC/minimal API integration, including model binding and JSON registration |

## Important: this is obfuscation, not encryption

CifraId is reversible by design. It is useful for hiding sequential integer IDs from API clients, but it is not cryptographic protection and must not be used for passwords, secrets, tokens, or tamper-proof identifiers.

## Packages

### Install `CifraId.Core`

Use this when you only need encoding/decoding and `System.Text.Json` support:

```shell
dotnet add package CifraId.Core
```

### Install `CifraId.AspNetCore`

Use this for ASP.NET Core APIs. It depends on `CifraId.Core`, so you do not need to install both:

```shell
dotnet add package CifraId.AspNetCore
```

## Quick start

### ASP.NET Core

```csharp
using CifraId.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCifraId(
    builder.Configuration,
    builder.Environment.IsDevelopment());

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Core-only usage

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CifraId.Extensions;
using CifraId.Services;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();
services.AddCifraIdServices(configuration, isDevelopment: false);

var provider = services.BuildServiceProvider();
var cifraId = provider.GetRequiredService<ICifraIdService>();

var encoded = cifraId.EncodeId(42);
var decoded = cifraId.DecodeId(encoded);
```

## Configuration

CifraId reads the `HashSettings` section:

```json
{
  "HashSettings": {
    "Enabled": true,
    "Salt": "CHANGE-ME-IN-PRODUCTION",
    "MinHashLength": 6,
    "Alphabet": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
    "Separators": "cfhistuCFHISTU"
  }
}
```

| Setting | Description | Default |
|---|---|---|
| `Enabled` | Turns encoding on or off in development | `true` |
| `Salt` | Salt for the reversible hash algorithm | development-only default |
| `MinHashLength` | Minimum internal hash length before numeric conversion | `6` |
| `Alphabet` | Character set for the internal hash algorithm | alphanumeric |
| `Separators` | Internal separator characters | `cfhistuCFHISTU` |

In non-development environments, CifraId always forces encoding on even if `Enabled` is set to `false`.

## Development-only disable mode

For debugging, development environments can switch to pass-through mode:

```json
{
  "HashSettings": {
    "Enabled": false
  }
}
```

When disabled in development:

- `NoOpEncoder` returns the original integer value
- `NoOpCifraIdService` returns plain numeric strings
- outbound IDs are no longer obfuscated

## DTO usage

```csharp
using CifraId.Attributes;

public sealed class OrderResponseDto
{
    [CifraId]
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    [CifraId]
    public OrderStatus Status { get; set; }

    [CifraId("encodedAgentId")]
    public int? AssignedAgentId { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3
}
```

Supported `[CifraId]` targets:

- `int`
- `int?`
- enum
- nullable enum

## Service registration

### `CifraId.Core`

```csharp
services.AddCifraIdServices(configuration, isDevelopment);
```

Registers:

- `HashSettings`
- `IEncoder`
- `ICifraIdService`
- `ICifraIdOutboundStringTransform`

### `CifraId.AspNetCore`

```csharp
services.AddCifraIdJsonConverter();
services.AddCifraIdModelBinder();

// or the one-line convenience method
services.AddCifraId(configuration, isDevelopment);
```

Registers:

- ASP.NET Core JSON converter wiring for MVC and minimal APIs
- `CifraIdModelBinderProvider`
- all core services when using `AddCifraId(...)`

## JSON behavior

For properties marked with `[CifraId]`:

- outbound JSON writes encoded numeric strings
- inbound JSON reads encoded strings back into `int` or enum values
- `JsonIgnore` is respected
- global naming policies such as camelCase are respected
- custom attribute names such as `[CifraId("encodedOrderId")]` override naming policy

Example:

```json
{
  "orderId": "676737415167",
  "customerName": "Alice",
  "status": "384955676768"
}
```

## Query-string model binding

`CifraId.AspNetCore` adds model binding for complex query DTOs that contain `[CifraId]` properties:

```csharp
public sealed class OrderQueryDto
{
    [CifraId]
    public int OrderId { get; set; }

    [CifraId]
    public OrderStatus? Status { get; set; }
}

[HttpGet("search")]
public ActionResult Search([FromQuery] OrderQueryDto query)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    return Ok(query);
}
```

Both query parameter name styles are supported:

```text
GET /api/orders/search?OrderId=676737415167
GET /api/orders/search?orderId=676737415167
```

Invalid encoded values add model-state errors.

## Manual service usage

```csharp
public sealed class OrderService
{
    private readonly ICifraIdService _cifraId;

    public OrderService(ICifraIdService cifraId) => _cifraId = cifraId;

    public string? EncodeId(int id) => _cifraId.EncodeId(id);
    public int? DecodeId(string encodedId) => _cifraId.DecodeId(encodedId);

    public string? EncodeStatus(OrderStatus status) => _cifraId.EncodeEnum(status);
    public OrderStatus? DecodeStatus(string encodedStatus) =>
        _cifraId.DecodeEnum<OrderStatus>(encodedStatus);
}
```

## Extensibility

You can override outbound string behavior for non-CifraId string properties via `ICifraIdOutboundStringTransform`:

```csharp
public sealed class CustomTransform : ICifraIdOutboundStringTransform
{
    public string Transform(PropertyInfo property, string value)
    {
        return property.Name == "Email" ? value.ToLowerInvariant() : value;
    }
}
```

Register your implementation before or instead of the default one.

## How it works

1. The raw integer is encoded with a Hashids-style reversible algorithm using the configured salt, alphabet, separators, and minimum length.
2. Each character in that internal alphanumeric hash is converted into a 2-digit numeric chunk by subtracting an ASCII offset of `30`.
3. The chunks are concatenated into the final numeric-only public value.
4. Decoding reverses the process and verifies the reconstructed internal hash before returning the integer.

Special case:

- `0` encodes to `"0"`
- `"0"` decodes to `0`

## Production guidance

- Use the same salt across all app instances that need to decode each other's IDs.
- Do not commit production salts to source control.
- Prefer environment variables, user secrets, Azure Key Vault, or another secure secret store.

Example:

```shell
HashSettings__Salt=your-strong-random-production-salt
```

## Migration from the old single package

If you previously referenced the single in-repo `CifraId` project:

- replace it with `CifraId.Core` if you only need encoding/services/JSON
- replace it with `CifraId.AspNetCore` for Web API usage

The public namespaces remain the same, so most application code does not need changes beyond package references.

## Running the sample API

```shell
dotnet run --project samples/CifraId.SampleApi
```

Endpoints:

- `GET /api/orders`
- `GET /api/orders/{encodedId}`
- `GET /api/orders/search?OrderId={encoded}`
- `GET /api/orders/manual`

## Running tests

```shell
dotnet test tests/CifraId.Core.Tests/CifraId.Core.Tests.csproj
dotnet test tests/CifraId.AspNetCore.Tests/CifraId.AspNetCore.Tests.csproj
```

## Packing NuGet packages

```shell
dotnet pack src/CifraId.Core/CifraId.Core.csproj -c Release -o ./nupkg
dotnet pack src/CifraId.AspNetCore/CifraId.AspNetCore.csproj -c Release -o ./nupkg
```

Both packages include:

- `net8.0` and `net10.0` builds
- XML documentation
- symbol packages (`.snupkg`)
- this README

## Troubleshooting

### `DecodeId` returns `null`

- the encoded value may be malformed
- the value may have been generated with a different salt
- the string may contain non-digits or invalid chunking

### IDs are not being encoded in responses

- verify the property has `[CifraId]`
- verify JSON registration is present
- verify `HashSettings:Enabled` is not disabled in development

### Query binding is not decoding

- verify `CifraId.AspNetCore` is referenced
- verify `AddCifraIdModelBinder()` or `AddCifraId(...)` is called
- verify the DTO is bound from query

## License

MIT
