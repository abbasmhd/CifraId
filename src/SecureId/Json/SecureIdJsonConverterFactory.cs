using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using SecureId.Attributes;
using SecureId.Services;
using SecureId.Transforms;

namespace SecureId.Json;

/// <summary>
/// A <see cref="JsonConverterFactory"/> that creates typed converters for object types
/// containing properties marked with <see cref="SecureIdAttribute"/>.
/// </summary>
public sealed class SecureIdJsonConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, bool> TypeCache = new();

    private readonly ISecureIdService _service;
    private readonly ISecureIdOutboundStringTransform _transform;

    /// <summary>
    /// Creates a new <see cref="SecureIdJsonConverterFactory"/>.
    /// </summary>
    public SecureIdJsonConverterFactory(
        ISecureIdService service,
        ISecureIdOutboundStringTransform transform)
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
            return false;

        if (typeof(IEnumerable).IsAssignableFrom(typeToConvert))
            return false;

        return TypeCache.GetOrAdd(typeToConvert, static type =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Any(p => p.GetCustomAttribute<SecureIdAttribute>() != null));
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(SecureIdJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, _service, _transform)!;
    }
}
