using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using CifraId.Configuration;
using CifraId.Encoding;
using CifraId.Services;
using CifraId.Transforms;

namespace CifraId.Extensions;

/// <summary>
/// Extension methods for registering core CifraId services into the DI container.
/// </summary>
public static class CifraIdCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers core CifraId services: configuration, encoder, and service layer.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration (reads the <c>HashSettings</c> section).</param>
    /// <param name="isDevelopment">
    /// Pass <c>true</c> only in the Development environment.
    /// When <c>false</c>, encoding is always forced on regardless of configuration.
    /// </param>
    public static IServiceCollection AddCifraIdServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment = false)
    {
        services.Configure<HashSettings>(configuration.GetSection(HashSettings.SectionName));

        if (!isDevelopment)
        {
            services.PostConfigure<HashSettings>(s => s.Enabled = true);
        }

        services.TryAddSingleton<ICifraIdOutboundStringTransform, DefaultCifraIdOutboundStringTransform>();

        services.AddSingleton<IEncoder>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<HashSettings>>().Value;
            return !settings.Enabled && isDevelopment
                ? new NoOpEncoder()
                : new Encoder(sp.GetRequiredService<IOptions<HashSettings>>());
        });

        services.AddSingleton<ICifraIdService>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<HashSettings>>().Value;
            return !settings.Enabled && isDevelopment
                ? new NoOpCifraIdService()
                : new CifraIdService(sp.GetRequiredService<IEncoder>());
        });

        return services;
    }
}
