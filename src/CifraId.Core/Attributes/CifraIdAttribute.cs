namespace CifraId.Attributes;

/// <summary>
/// Marks a property or parameter for automatic CifraId encoding and decoding.
/// When applied to <c>int</c>, <c>int?</c>, enum, or nullable enum properties,
/// values are serialized as obfuscated numeric strings in JSON and decoded
/// automatically during deserialization and model binding.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CifraIdAttribute : Attribute
{
    /// <summary>
    /// Gets the custom JSON property name to use during serialization.
    /// When <c>null</c>, the default naming policy applies.
    /// </summary>
    public string? JsonPropertyName { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CifraIdAttribute"/>
    /// without a custom JSON property name.
    /// </summary>
    public CifraIdAttribute() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CifraIdAttribute"/>
    /// with a custom JSON property name.
    /// </summary>
    /// <param name="jsonPropertyName">
    /// The JSON property name to use during serialization and deserialization.
    /// </param>
    public CifraIdAttribute(string jsonPropertyName)
    {
        JsonPropertyName = jsonPropertyName;
    }
}
