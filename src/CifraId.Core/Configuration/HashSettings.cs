namespace CifraId.Configuration;

/// <summary>
/// Configuration options for the CifraId hashing behavior.
/// Bind to the <c>HashSettings</c> configuration section.
/// </summary>
/// <remarks>
/// <para>In non-development environments, <see cref="Enabled"/> is always forced to <c>true</c>
/// regardless of the configuration value.</para>
/// <para><b>IMPORTANT:</b> Override <see cref="Salt"/> in production using environment variables,
/// Azure Key Vault, or another secure configuration source. The default salt is for development only.</para>
/// </remarks>
public sealed class HashSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "HashSettings";

    /// <summary>
    /// Whether CifraId encoding is enabled. In non-development environments this is
    /// always forced to <c>true</c> regardless of configuration.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The salt used by the internal hashing algorithm.
    /// <b>Must</b> be overridden in production.
    /// </summary>
    public string Salt { get; set; } = "CifraId-Dev-Salt-CHANGE-IN-PRODUCTION";

    /// <summary>
    /// The minimum length of the internal hash output before ASCII-offset conversion.
    /// </summary>
    public int MinHashLength { get; set; } = 6;

    /// <summary>
    /// The alphabet of characters used by the internal hashing algorithm.
    /// Must contain at least 16 unique characters.
    /// </summary>
    public string Alphabet { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    /// <summary>
    /// The separator characters used by the internal hashing algorithm.
    /// </summary>
    public string Separators { get; set; } = "cfhistuCFHISTU";
}
