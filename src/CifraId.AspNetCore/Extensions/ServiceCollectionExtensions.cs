using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CifraId.Json;
using CifraId.ModelBinding;

namespace CifraId.Extensions;

/// <summary>
/// Extension methods for registering ASP.NET Core CifraId integration.
/// </summary>
public static class CifraIdAspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="CifraIdJsonConverterFactory"/> into both MVC and
    /// minimal-API JSON serializer options.
    /// </summary>
    public static IServiceCollection AddCifraIdJsonConverter(this IServiceCollection services)
    {
        services.AddSingleton<CifraIdJsonConverterFactory>();
        services.AddSingleton<IPostConfigureOptions<JsonOptions>, CifraIdMvcJsonOptionsSetup>();
        services.AddSingleton<IPostConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
            CifraIdHttpJsonOptionsSetup>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="CifraIdModelBinderProvider"/> so that models
    /// with <c>[CifraId]</c> properties are decoded automatically from query strings.
    /// </summary>
    public static IServiceCollection AddCifraIdModelBinder(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            if (!options.ModelBinderProviders.Any(p => p is CifraIdModelBinderProvider))
                options.ModelBinderProviders.Insert(0, new CifraIdModelBinderProvider());
        });
        return services;
    }

    /// <summary>
    /// Convenience method that registers all CifraId services, JSON converters,
    /// and model binder support in a single call.
    /// </summary>
    public static IServiceCollection AddCifraId(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        services.AddCifraIdServices(configuration, isDevelopment);
        services.AddCifraIdJsonConverter();
        services.AddCifraIdModelBinder();
        return services;
    }
}

internal sealed class CifraIdMvcJsonOptionsSetup : IPostConfigureOptions<JsonOptions>
{
    private readonly CifraIdJsonConverterFactory _factory;

    public CifraIdMvcJsonOptionsSetup(CifraIdJsonConverterFactory factory) => _factory = factory;

    public void PostConfigure(string? name, JsonOptions options)
    {
        if (!options.JsonSerializerOptions.Converters.Any(c => c is CifraIdJsonConverterFactory))
            options.JsonSerializerOptions.Converters.Add(_factory);
    }
}

internal sealed class CifraIdHttpJsonOptionsSetup
    : IPostConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>
{
    private readonly CifraIdJsonConverterFactory _factory;

    public CifraIdHttpJsonOptionsSetup(CifraIdJsonConverterFactory factory) => _factory = factory;

    public void PostConfigure(string? name, Microsoft.AspNetCore.Http.Json.JsonOptions options)
    {
        if (!options.SerializerOptions.Converters.Any(c => c is CifraIdJsonConverterFactory))
            options.SerializerOptions.Converters.Add(_factory);
    }
}
