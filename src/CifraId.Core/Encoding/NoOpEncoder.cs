namespace CifraId.Encoding;

/// <summary>
/// A pass-through encoder that performs no obfuscation.
/// Used when CifraId encoding is disabled in development mode.
/// </summary>
public sealed class NoOpEncoder : IEncoder
{
    /// <inheritdoc />
    public long? Encode(int number) => number;

    /// <inheritdoc />
    public int? Decode(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return null;
        return int.TryParse(number, out var result) ? result : null;
    }
}
