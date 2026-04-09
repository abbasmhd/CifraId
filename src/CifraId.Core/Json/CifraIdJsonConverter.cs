using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CifraId.Attributes;
using CifraId.Services;
using CifraId.Transforms;

namespace CifraId.Json;

/// <summary>
/// Typed JSON converter for objects containing <see cref="CifraIdAttribute"/> properties.
/// Encodes marked properties on write and decodes them on read.
/// </summary>
/// <typeparam name="T">The object type being serialized. Must be a class with a parameterless constructor.</typeparam>
public sealed class CifraIdJsonConverter<T> : JsonConverter<T> where T : class
{
    private static readonly ConcurrentDictionary<Type, PropMeta[]> MetadataCache = new();

    private readonly ICifraIdService _service;
    private readonly ICifraIdOutboundStringTransform _transform;

    /// <summary>Creates a new converter instance.</summary>
    public CifraIdJsonConverter(ICifraIdService service, ICifraIdOutboundStringTransform transform)
    {
        _service = service;
        _transform = transform;
    }

    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Expected StartObject, got {reader.TokenType}.");

        var result = (T)Activator.CreateInstance(typeof(T))!;
        var metadata = GetMetadata();
        var propertyMap = BuildPropertyMap(metadata, options.PropertyNamingPolicy);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return result;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Expected PropertyName, got {reader.TokenType}.");

            var jsonPropName = reader.GetString()!;
            reader.Read();

            if (!propertyMap.TryGetValue(jsonPropName, out var meta))
            {
                reader.Skip();
                continue;
            }

            if (meta.IsIgnored)
            {
                reader.Skip();
                continue;
            }

            if (meta.CifraIdAttr is not null)
                ReadCifraIdProperty(ref reader, result, meta);
            else
                ReadRegularProperty(ref reader, result, meta, options);
        }

        throw new JsonException("Unexpected end of JSON object.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        var metadata = GetMetadata();
        var namingPolicy = options.PropertyNamingPolicy;

        foreach (var meta in metadata)
        {
            if (meta.IsIgnored) continue;

            var jsonName = ResolveJsonName(meta, namingPolicy);
            var propValue = meta.Property.GetValue(value);

            writer.WritePropertyName(jsonName);

            if (meta.CifraIdAttr is not null)
                WriteCifraIdValue(writer, propValue, meta.Property.PropertyType);
            else if (meta.Property.PropertyType == typeof(string) && propValue is string str)
                writer.WriteStringValue(_transform.Transform(meta.Property, str));
            else
                JsonSerializer.Serialize(writer, propValue, meta.Property.PropertyType, options);
        }

        writer.WriteEndObject();
    }

    private void ReadCifraIdProperty(ref Utf8JsonReader reader, T target, PropMeta meta)
    {
        var property = meta.Property;
        var propType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;
        var isNullable = Nullable.GetUnderlyingType(propType) is not null || !propType.IsValueType;

        if (reader.TokenType == JsonTokenType.Null)
        {
            if (isNullable && property.CanWrite)
                property.SetValue(target, null);
            return;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            reader.Skip();
            return;
        }

        var encodedValue = reader.GetString();
        if (string.IsNullOrEmpty(encodedValue))
        {
            if (isNullable && property.CanWrite)
                property.SetValue(target, null);
            return;
        }

        if (underlyingType.IsEnum)
        {
            var method = typeof(ICifraIdService)
                .GetMethod(nameof(ICifraIdService.DecodeEnum))!
                .MakeGenericMethod(underlyingType);
            var decoded = method.Invoke(_service, [encodedValue]);
            if (decoded is not null && property.CanWrite)
                property.SetValue(target, decoded);
            else if (isNullable && property.CanWrite)
                property.SetValue(target, null);
        }
        else if (underlyingType == typeof(int))
        {
            var decoded = _service.DecodeId(encodedValue);
            if (decoded is not null && property.CanWrite)
                property.SetValue(target, decoded.Value);
            else if (isNullable && property.CanWrite)
                property.SetValue(target, null);
        }
    }

    private static void ReadRegularProperty(
        ref Utf8JsonReader reader, T target, PropMeta meta, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize(ref reader, meta.Property.PropertyType, options);
        if (meta.Property.CanWrite)
            meta.Property.SetValue(target, value);
    }

    private void WriteCifraIdValue(Utf8JsonWriter writer, object? value, Type propertyType)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        string? encoded;

        if (underlyingType.IsEnum)
            encoded = _service.EncodeId(Convert.ToInt32(value));
        else if (underlyingType == typeof(int))
            encoded = _service.EncodeId((int)value);
        else
            encoded = value.ToString();

        if (encoded is not null)
            writer.WriteStringValue(encoded);
        else
            writer.WriteNullValue();
    }

    private static PropMeta[] GetMetadata() =>
        MetadataCache.GetOrAdd(typeof(T), static type =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new PropMeta
                {
                    Property = p,
                    CifraIdAttr = p.GetCustomAttribute<CifraIdAttribute>(),
                    IsIgnored = p.GetCustomAttribute<JsonIgnoreAttribute>() is not null,
                    ExplicitJsonName = p.GetCustomAttribute<CifraIdAttribute>()?.JsonPropertyName
                                      ?? p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name,
                })
                .ToArray());

    private static Dictionary<string, PropMeta> BuildPropertyMap(
        PropMeta[] metadata, JsonNamingPolicy? namingPolicy)
    {
        var map = new Dictionary<string, PropMeta>(StringComparer.OrdinalIgnoreCase);
        foreach (var meta in metadata)
        {
            if (meta.IsIgnored) continue;
            var name = ResolveJsonName(meta, namingPolicy);
            map[name] = meta;
            map[meta.Property.Name] = meta;
        }
        return map;
    }

    private static string ResolveJsonName(PropMeta meta, JsonNamingPolicy? namingPolicy)
    {
        if (!string.IsNullOrEmpty(meta.ExplicitJsonName))
            return meta.ExplicitJsonName!;

        return namingPolicy?.ConvertName(meta.Property.Name) ?? meta.Property.Name;
    }

    private sealed class PropMeta
    {
        public required PropertyInfo Property { get; init; }
        public CifraIdAttribute? CifraIdAttr { get; init; }
        public bool IsIgnored { get; init; }
        public string? ExplicitJsonName { get; init; }
    }
}
