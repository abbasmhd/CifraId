using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CifraId.Attributes;
using CifraId.Services;
using CifraId.Transforms;

namespace CifraId.Json;

/// <summary>
/// A <see cref="JsonConverterFactory"/> that creates typed converters for object types
/// containing properties marked with <see cref="CifraIdAttribute"/>.
/// </summary>
public sealed class CifraIdJsonConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, bool> TypeCache = new();

    private readonly ICifraIdService _service;
    private readonly ICifraIdOutboundStringTransform _transform;

    /// <summary>
    /// Creates a new <see cref="CifraIdJsonConverterFactory"/>.
    /// </summary>
    public CifraIdJsonConverterFactory(
        ICifraIdService service,
        ICifraIdOutboundStringTransform transform)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert.IsPrimitive || typeToConvert == typeof(string) ||
            typeToConvert == typeof(decimal) || typeToConvert == typeof(DateTime) ||
            typeToConvert == typeof(DateTimeOffset) || typeToConvert == typeof(Guid) ||
            typeToConvert == typeof(TimeSpan) || typeToConvert.IsEnum ||
            typeToConvert.IsArray || typeToConvert.IsValueType)
        {
            return false;
        }

        if (typeof(IEnumerable).IsAssignableFrom(typeToConvert))
        {
            return false;
        }

        return TypeCache.GetOrAdd(typeToConvert, static type =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(p => p.GetCustomAttribute<CifraIdAttribute>() != null));
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(CifraIdJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, _service, _transform)!;
    }
}
