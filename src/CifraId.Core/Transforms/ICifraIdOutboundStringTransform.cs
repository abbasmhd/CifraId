using System.Reflection;

namespace CifraId.Transforms;

/// <summary>
/// Extensibility hook for transforming outbound string property values during
/// JSON serialization. Applied to non-CifraId string properties on types that
/// contain at least one <c>[CifraId]</c> property.
/// </summary>
/// <remarks>
/// The default implementation (<see cref="DefaultCifraIdOutboundStringTransform"/>)
/// returns the value unchanged. Register a custom implementation to apply masking,
/// formatting, or other transformations.
/// </remarks>
public interface ICifraIdOutboundStringTransform
{
    /// <summary>
    /// Transforms the outbound string value for the given property.
    /// </summary>
    /// <param name="property">The property being serialized.</param>
    /// <param name="value">The original string value.</param>
    /// <returns>The transformed string value.</returns>
    string Transform(PropertyInfo property, string value);
}
