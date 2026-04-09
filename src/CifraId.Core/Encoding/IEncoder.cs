namespace CifraId.Encoding;

/// <summary>
/// Provides reversible encoding and decoding of integer values
/// into obfuscated numeric representations.
/// </summary>
public interface IEncoder
{
    /// <summary>
    /// Encodes an integer into an obfuscated numeric value.
    /// </summary>
    /// <param name="number">The integer to encode. Must be non-negative.</param>
    /// <returns>
    /// The encoded numeric value, or <c>null</c> if encoding fails
    /// (e.g. negative input).
    /// </returns>
    long? Encode(int number);

    /// <summary>
    /// Decodes an obfuscated numeric string back into the original integer.
    /// </summary>
    /// <param name="number">The encoded numeric string to decode.</param>
    /// <returns>
    /// The decoded integer, or <c>null</c> if the input is <c>null</c>,
    /// empty, malformed, or does not represent a valid encoded value.
    /// </returns>
    int? Decode(string? number);
}
