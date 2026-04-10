using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using CifraId.Attributes;
using CifraId.Services;

namespace CifraId.ModelBinding;

/// <summary>
/// ASP.NET Core model binder for models containing <see cref="CifraIdAttribute"/> properties.
/// Decodes CifraId values from query string and other value providers automatically.
/// </summary>
public sealed class CifraIdModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var service = bindingContext.HttpContext.RequestServices
            .GetRequiredService<ICifraIdService>();

        var modelType = bindingContext.ModelType;
        var model = Activator.CreateInstance(modelType)!;
        var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanWrite)
            {
                continue;
            }

            var cifraIdAttr = property.GetCustomAttribute<CifraIdAttribute>();
            var propertyName = property.Name;

            var valueResult = bindingContext.ValueProvider.GetValue(propertyName);
            if (valueResult == ValueProviderResult.None && propertyName.Length > 0)
            {
                var camelCase = char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
                valueResult = bindingContext.ValueProvider.GetValue(camelCase);
            }

            if (valueResult == ValueProviderResult.None)
            {
                continue;
            }

            var rawValue = valueResult.FirstValue;

            if (cifraIdAttr is not null)
            {
                BindCifraIdProperty(bindingContext, service, model, property, rawValue);
            }
            else
            {
                BindRegularProperty(model, property, rawValue);
            }
        }

        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }

    private static void BindCifraIdProperty(
        ModelBindingContext bindingContext,
        ICifraIdService service,
        object model,
        PropertyInfo property,
        string? rawValue)
    {
        var isNullable = Nullable.GetUnderlyingType(property.PropertyType) is not null;

        if (string.IsNullOrEmpty(rawValue))
        {
            if (isNullable)
            {
                property.SetValue(model, null);
            }

            return;
        }

        var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (underlyingType.IsEnum)
        {
            var method = typeof(ICifraIdService)
                .GetMethod(nameof(ICifraIdService.DecodeEnum))!
                .MakeGenericMethod(underlyingType);
            var decoded = method.Invoke(service, [rawValue]);

            if (decoded is not null)
            {
                property.SetValue(model, decoded);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                    property.Name, $"Invalid encoded value for '{property.Name}'.");
            }
        }
        else if (underlyingType == typeof(int))
        {
            var decoded = service.DecodeId(rawValue);
            if (decoded is not null)
            {
                property.SetValue(model, isNullable ? decoded : decoded.Value);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                    property.Name, $"Invalid encoded value for '{property.Name}'.");
            }
        }
    }

    private static void BindRegularProperty(object model, PropertyInfo property, string? rawValue)
    {
        if (rawValue is null)
        {
            return;
        }

        try
        {
            var converter = TypeDescriptor.GetConverter(property.PropertyType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                var converted = converter.ConvertFromInvariantString(rawValue);
                property.SetValue(model, converted);
            }
        }
        catch
        {
            // Non-CifraId binding failures are silently ignored; ASP.NET Core
            // validation will catch these through other mechanisms.
        }
    }
}
