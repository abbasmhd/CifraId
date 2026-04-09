# QED SecureId

ASP.NET Core and System.Text.Json support for **reversible obfuscated ID serialization** and model binding.

SecureId hides raw integer database IDs from API consumers by encoding them as numeric-looking strings. Mark your DTO properties with `[SecureId]` and the library handles JSON serialization, deserialization, and query-string model binding automatically.

## Important: This Is Obfuscation, Not Encryption

SecureId uses a **reversible** Hashids-style algorithm. It is designed to prevent casual ID enumeration and hide internal database structure from clients. It is **not** cryptographically secure and must **never** be used for passwords, secrets, tokens, or any security-sensitive data.

If you need tamper-proof or non-reversible identifiers, use UUIDs, GUIDs, or cryptographic HMACs instead.

## Installation

```shell
dotnet add package SecureId
```

## Quick Start

### 1. Register services in `Program.cs`

```csharp
using SecureId.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Registers encoder, service, JSON converter, and model binder.
builder.Services.AddSecureId(
    builder.Configuration,
    builder.Environment.IsDevelopment());

var app = builder.Build();
app.MapControllers();
app.Run();
```

### 2. Add configuration

```json
// appsettings.json
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

### 3. Mark DTO properties

```csharp
using SecureId.Attributes;

public class OrderResponseDto
{
    [SecureId]
    public int OrderId { get; set; }

    public string CustomerName { get; set; } = "";

    [SecureId]
    public OrderStatus Status { get; set; }

    [SecureId]
    public int? AssignedAgentId { get; set; }
}

public enum OrderStatus { Pending, Processing, Shipped, Delivered }
```

### 4. Use in controllers as usual

```csharp
[HttpGet]
public ActionResult<OrderResponseDto[]> GetOrders()
{
    return Ok(new[]
    {
        new OrderResponseDto
        {
            OrderId = 42,
            CustomerName = "Alice",
            Status = OrderStatus.Shipped,
        }
    });
}
```

The JSON response automatically encodes `OrderId` and `Status`:

```json
{
  "orderId": "676737415167",
  "customerName": "Alice",
  "status": "384955676768"
}
```

Inbound JSON with encoded strings is decoded back into integers and enums automatically.

## Configuration

| Property | Type | Default | Description |
|---|---|---|---|
| `Enabled` | `bool` | `true` | Enable/disable encoding. In non-development environments this is always forced to `true`. |
| `Salt` | `string` | `"SecureId-Dev-Salt-..."` | Salt for the hashing algorithm. **Must be overridden in production.** |
| `MinHashLength` | `int` | `6` | Minimum length of the internal hash before numeric conversion. |
| `Alphabet` | `string` | `a-zA-Z0-9` | Characters used by the internal hashing algorithm. |
| `Separators` | `string` | `"cfhistuCFHISTU"` | Separator characters used internally. |

### Production Salt Management

**Never commit production salts to source control.**

Use environment variables, Azure Key Vault, AWS Secrets Manager, or your platform's secret management:

```shell
# Environment variable override
export HashSettings__Salt="your-strong-random-production-salt"
```

```csharp
// Or use Azure Key Vault, user secrets, etc.
builder.Configuration.AddAzureKeyVault(...);
```

### Development-Only Disable Mode

In development, you can disable encoding for easier debugging:

```json
// appsettings.Development.json
{
  "HashSettings": {
    "Enabled": false
  }
}
```

When disabled in development, `NoOpEncoder` and `NoOpSecureIdService` are used. IDs pass through as plain integers. **This only works when `isDevelopment: true` is passed during registration.** Non-development environments always force encoding on.

## Service Registration

SecureId provides granular and convenience registration methods:

```csharp
// All-in-one (recommended)
services.AddSecureId(configuration, isDevelopment);

// Or register individually
services.AddSecureIdServices(configuration, isDevelopment);
services.AddSecureIdJsonConverter();
services.AddSecureIdModelBinder();
```

| Method | Registers |
|---|---|
| `AddSecureIdServices` | `IEncoder`, `ISecureIdService`, `HashSettings`, `ISecureIdOutboundStringTransform` |
| `AddSecureIdJsonConverter` | `SecureIdJsonConverterFactory` into MVC and minimal-API JSON options |
| `AddSecureIdModelBinder` | `SecureIdModelBinderProvider` into MVC options |
| `AddSecureId` | All of the above |

## The `[SecureId]` Attribute

```csharp
// Basic usage
[SecureId]
public int Id { get; set; }

// With custom JSON property name
[SecureId("encodedCustomerId")]
public int CustomerId { get; set; }
```

Supported property types:

- `int`
- `int?`
- `enum` (encoded via underlying integer value)
- `Nullable<TEnum>`

The attribute works with:
- **JSON serialization** — encodes on write, decodes on read
- **Model binding** — decodes query-string values automatically
- **`JsonIgnore`** — respected; ignored properties are skipped
- **`JsonNamingPolicy`** — camelCase and other policies are applied unless a custom name is specified

## Manual Service Usage

Inject `ISecureIdService` when you need to encode/decode outside of automatic serialization:

```csharp
public class MyService
{
    private readonly ISecureIdService _secureId;

    public MyService(ISecureIdService secureId) => _secureId = secureId;

    public string GetEncodedId(int id) => _secureId.EncodeId(id)!;

    public int? ParseEncodedId(string encoded) => _secureId.DecodeId(encoded);

    // Enum support
    public string GetEncodedStatus(OrderStatus status) =>
        _secureId.EncodeEnum(status)!;

    public OrderStatus? ParseEncodedStatus(string encoded) =>
        _secureId.DecodeEnum<OrderStatus>(encoded);

    // Batch operations
    public string?[] EncodeMany(params int[] ids) =>
        _secureId.EncodeIds(ids);
}
```

## Query-String Model Binding

Models with `[SecureId]` properties are automatically decoded from query strings:

```csharp
public class OrderQueryDto
{
    [SecureId]
    public int OrderId { get; set; }

    [SecureId]
    public OrderStatus? Status { get; set; }
}

[HttpGet("search")]
public ActionResult Search([FromQuery] OrderQueryDto query)
{
    // query.OrderId is already decoded from the encoded query-string value
    if (!ModelState.IsValid) return BadRequest(ModelState);
    // ...
}
```

Both `PascalCase` and `camelCase` query parameter names are supported:

```
GET /api/orders/search?OrderId=676737415167
GET /api/orders/search?orderId=676737415167
```

Invalid encoded values produce model-state errors.

## Extensibility

### Custom Outbound String Transform

Register a custom `ISecureIdOutboundStringTransform` to transform non-SecureId string properties on types processed by the converter:

```csharp
public class MaskingTransform : ISecureIdOutboundStringTransform
{
    public string Transform(PropertyInfo property, string value)
    {
        if (property.Name == "Email")
            return MaskEmail(value);
        return value;
    }
}

// Register before AddSecureId
services.AddSingleton<ISecureIdOutboundStringTransform, MaskingTransform>();
```

## How It Works

1. **Internal hashing**: The integer is encoded using a Hashids-style algorithm with your configured salt, alphabet, separators, and minimum length. This produces a short alphanumeric string.

2. **Numeric conversion**: Each character of the alphanumeric hash is converted to a 2-digit number by subtracting an ASCII offset (30). The digits are concatenated to form the final numeric-only encoded value.

3. **Decoding**: The numeric string is split into 2-digit chunks, each chunk is converted back to a character by adding the ASCII offset, and the resulting alphanumeric string is decoded with the Hashids algorithm.

4. **Special case**: `0` encodes to `"0"` and decodes back to `0` without going through the algorithm.

## Running the Sample API

```shell
cd samples/SecureId.SampleApi
dotnet run
```

Then visit:
- `GET /api/orders` — list orders with encoded IDs
- `GET /api/orders/{encodedId}` — get order by encoded ID
- `GET /api/orders/search?OrderId={encoded}` — query-string binding demo
- `GET /api/orders/manual` — manual encode/decode example

## Running Tests

```shell
dotnet test
```

## Building the NuGet Package

```shell
dotnet pack src/SecureId/SecureId.csproj -c Release -o ./nupkg
```

The package includes:
- Compiled assemblies for `net8.0` and `net10.0`
- XML documentation
- Symbol package (`.snupkg`)
- This README

## Troubleshooting

### Encoded values change between environments
Each environment **must** use the same `Salt`. If the salt differs, encoded values will not decode correctly. Use a shared secret store for production salt.

### IDs are not being encoded in JSON responses
- Verify `AddSecureId` (or `AddSecureIdJsonConverter`) is called in `Program.cs`
- Verify the property has the `[SecureId]` attribute
- Verify the property type is `int`, `int?`, an enum, or a nullable enum
- Check that `HashSettings:Enabled` is `true` (or you're not in development with it disabled)

### Query-string binding doesn't decode values
- Verify `AddSecureIdModelBinder()` is called (included in `AddSecureId`)
- Use `[FromQuery]` on the parameter
- Check both PascalCase and camelCase query parameter names

### `DecodeId` returns `null`
- The encoded string may have been produced with a different salt
- The string may be malformed (non-numeric characters, odd length)
- Zero-length or whitespace input always returns `null`

### Development mode shows plain integer IDs
This is expected when `HashSettings:Enabled = false` in development. The `NoOpEncoder` passes values through unchanged.

## License

MIT
