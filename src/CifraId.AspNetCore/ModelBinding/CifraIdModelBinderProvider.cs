using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CifraId.Attributes;

namespace CifraId.ModelBinding;

/// <summary>
/// Provides <see cref="CifraIdModelBinder"/> for model types that contain
/// at least one property marked with <see cref="CifraIdAttribute"/>.
/// </summary>
public sealed class CifraIdModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Metadata.IsComplexType || context.Metadata.IsCollectionType)
            return null;

        var modelType = context.Metadata.ModelType;
        var hasCifraIdProperties = modelType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetCustomAttribute<CifraIdAttribute>() is not null);

        return hasCifraIdProperties ? new CifraIdModelBinder() : null;
    }
}
