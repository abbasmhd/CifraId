using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SecureId.Attributes;

namespace SecureId.ModelBinding;

/// <summary>
/// Provides <see cref="SecureIdModelBinder"/> for model types that contain
/// at least one property marked with <see cref="SecureIdAttribute"/>.
/// </summary>
public sealed class SecureIdModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Metadata.IsComplexType || context.Metadata.IsCollectionType)
            return null;

        var modelType = context.Metadata.ModelType;
        var hasSecureIdProperties = modelType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetCustomAttribute<SecureIdAttribute>() is not null);

        return hasSecureIdProperties ? new SecureIdModelBinder() : null;
    }
}
