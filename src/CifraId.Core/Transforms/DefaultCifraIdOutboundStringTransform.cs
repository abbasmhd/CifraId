using System.Reflection;

namespace CifraId.Transforms;

/// <summary>
/// Default pass-through implementation that returns string values unchanged.
/// </summary>
public sealed class DefaultCifraIdOutboundStringTransform : ICifraIdOutboundStringTransform
{
    /// <inheritdoc />
    public string Transform(PropertyInfo property, string value) => value;
}
