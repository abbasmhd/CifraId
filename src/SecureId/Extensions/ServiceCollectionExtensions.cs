using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SecureId.Configuration;
using SecureId.Encoding;
using SecureId.Json;
using SecureId.ModelBinding;
using SecureId.Services;
using SecureId.Transforms;

namespace SecureId.Extensions;

/// <summary>
/// Extension methods for registering SecureId services into the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core SecureId services: configuration, encoder, and service layer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (reads the <c>HashSettings</c> section).</param>
    /// <param name="isDevelopment">
    /// Pass <c>true</c> only in the Development environment.
    /// When <c>false</c>, encoding is always forced on regardless of configuration.
    /// </param>
    public static IServiceCollection AddSecureIdServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        services.Configure<HashSettings>(configuration.GetSection(HashSettings.SectionName));

        if (!isDevelopment)
            services.PostConfigure<HashSettings>(s => s.Enabled = true);

        services.TryAddSingleton<ISecureIdOutboundStringTransform, DefaultSecureIdOutboundStringTransform>();

        services.AddSingleton<IEncoder>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<HashSettings>>().Value;
            return !settings.Enabled && isDevelopment
                ? new NoOpEncoder()
                : new Encoder(sp.GetRequiredService<IOptions<HashSettings>>());
        });

        services.AddSingleton<ISecureIdService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<HashSettings>>().Value;
            return !settings.Enabled && isDevelopment
                ? new NoOpSecureIdService()
                : new SecureIdService(sp.GetRequiredService<IEncoder>());
        });

        return services;
    }

    /// <summary>
    /// Registers the <see cref="SecureIdJsonConverterFactory"/> into both MVC and
    /// minimal-API JSON serializer options.
    /// </summary>
    public static IServiceCollection AddSecureIdJsonConverter(this IServiceCollection services)
    {
        services.AddSingleton<SecureIdJsonConverterFactory>();
        services.AddSingleton<IPostConfigureOptions<JsonOptions>, SecureIdMvcJsonOptionsSetup>();
        services.AddSingleton<IPostConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
            SecureIdHttpJsonOptionsSetup>();
        return services;
    }

    /// <summary>
    /// Registers the <see cref="SecureIdModelBinderProvider"/> so that models
    /// with <c>[SecureId]</c> properties are decoded automatically from query strings.
    /// </summary>
    public static IServiceCollection AddSecureIdModelBinder(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            if (!options.ModelBinderProviders.Any(p => p is SecureIdModelBinderProvider))
                options.ModelBinderProviders.Insert(0, new SecureIdModelBinderProvider());
        });
        return services;
    }

    /// <summary>
    /// Convenience method that registers all SecureId services, JSON converters,
    /// and model binder support in a single call.
    /// </summary>
    public static IServiceCollection AddSecureId(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        services.AddSecureIdServices(configuration, isDevelopment);
        services.AddSecureIdJsonConverter();
        services.AddSecureIdModelBinder();
        return services;
    }
}

internal sealed class SecureIdMvcJsonOptionsSetup : IPostConfigureOptions<JsonOptions>
{
    private readonly SecureIdJsonConverterFactory _factory;

    public SecureIdMvcJsonOptionsSetup(SecureIdJsonConverterFactory factory) => _factory = factory;

    public void PostConfigure(string? name, JsonOptions options)
    {
        if (!options.JsonSerializerOptions.Converters.Any(c => c is SecureIdJsonConverterFactory))
            options.JsonSerializerOptions.Converters.Add(_factory);
    }
}

internal sealed class SecureIdHttpJsonOptionsSetup
    : IPostConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>
{
    private readonly SecureIdJsonConverterFactory _factory;

    public SecureIdHttpJsonOptionsSetup(SecureIdJsonConverterFactory factory) => _factory = factory;

    public void PostConfigure(string? name, Microsoft.AspNetCore.Http.Json.JsonOptions options)
    {
        if (!options.SerializerOptions.Converters.Any(c => c is SecureIdJsonConverterFactory))
            options.SerializerOptions.Converters.Add(_factory);
    }
}
