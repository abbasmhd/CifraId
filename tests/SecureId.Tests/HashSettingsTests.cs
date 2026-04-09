using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SecureId.Configuration;
using SecureId.Encoding;
using SecureId.Extensions;
using SecureId.Services;

namespace SecureId.Tests;

public class HashSettingsTests
{
    [Fact]
    public void Settings_Bind_From_Configuration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HashSettings:Salt"] = "custom-salt",
                ["HashSettings:Enabled"] = "true",
                ["HashSettings:MinHashLength"] = "8",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSecureIdServices(config, isDevelopment: false);
        var provider = services.BuildServiceProvider();

        var settings = provider.GetRequiredService<IOptions<HashSettings>>().Value;
        Assert.Equal("custom-salt", settings.Salt);
        Assert.True(settings.Enabled);
        Assert.Equal(8, settings.MinHashLength);
    }

    [Fact]
    public void Development_Disabled_Uses_NoOp_Encoder()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HashSettings:Enabled"] = "false",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSecureIdServices(config, isDevelopment: true);
        var provider = services.BuildServiceProvider();

        Assert.IsType<NoOpEncoder>(provider.GetRequiredService<IEncoder>());
    }

    [Fact]
    public void Development_Disabled_Uses_NoOp_Service()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HashSettings:Enabled"] = "false",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSecureIdServices(config, isDevelopment: true);
        var provider = services.BuildServiceProvider();

        Assert.IsType<NoOpSecureIdService>(provider.GetRequiredService<ISecureIdService>());
    }

    [Fact]
    public void Development_Enabled_Uses_Real_Encoder()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HashSettings:Enabled"] = "true",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSecureIdServices(config, isDevelopment: true);
        var provider = services.BuildServiceProvider();

        Assert.IsType<Encoder>(provider.GetRequiredService<IEncoder>());
    }

    [Fact]
    public void NonDevelopment_Forces_Enabled_Even_When_Config_Says_False()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["HashSettings:Enabled"] = "false",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSecureIdServices(config, isDevelopment: false);
        var provider = services.BuildServiceProvider();

        Assert.IsType<Encoder>(provider.GetRequiredService<IEncoder>());
        Assert.IsType<SecureIdService>(provider.GetRequiredService<ISecureIdService>());
    }

    [Fact]
    public void Default_Settings_Have_Safe_Defaults()
    {
        var settings = new HashSettings();

        Assert.True(settings.Enabled);
        Assert.False(string.IsNullOrEmpty(settings.Salt));
        Assert.True(settings.MinHashLength > 0);
        Assert.True(settings.Alphabet.Length >= 16);
        Assert.False(string.IsNullOrEmpty(settings.Separators));
    }
}
