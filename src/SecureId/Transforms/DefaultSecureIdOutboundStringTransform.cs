using System.Reflection;

namespace SecureId.Transforms;

/// <summary>
/// Default pass-through implementation that returns string values unchanged.
/// </summary>
public sealed class DefaultSecureIdOutboundStringTransform : ISecureIdOutboundStringTransform
{
    /// <inheritdoc />
    public string Transform(PropertyInfo property, string value) => value;
}
