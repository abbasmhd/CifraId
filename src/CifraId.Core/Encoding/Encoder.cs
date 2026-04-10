using System.Text;
using Microsoft.Extensions.Options;
using CifraId.Configuration;

namespace CifraId.Encoding;

/// <summary>
/// Reversible encoder that obfuscates integer values into numeric-only strings.
/// Uses a Hashids-style algorithm internally then converts the alphanumeric output
/// to a digits-only representation via fixed ASCII-offset conversion.
/// </summary>
/// <remarks>
/// <para>This is <b>obfuscation</b>, not encryption. It hides raw database IDs from
/// API consumers but must not be used for passwords, secrets, or any
/// cryptographic purpose.</para>
/// <para>Encoding rules:</para>
/// <list type="bullet">
///   <item><c>Encode(0)</c> returns <c>0</c>.</item>
///   <item>Negative input returns <c>null</c>.</item>
///   <item>Each character of the internal hash is converted to a 2-digit number
///         by subtracting an ASCII offset of 30.</item>
/// </list>
/// </remarks>
public sealed class Encoder : IEncoder
{
    private const int AsciiOffset = 30;
    private readonly HashidsAlgorithm _hashids;

    /// <summary>
    /// Initializes a new <see cref="Encoder"/> from the provided hash settings.
    /// </summary>
    public Encoder(IOptions<HashSettings> options)
    {
        var settings = options.Value;
        _hashids = new HashidsAlgorithm(
            settings.Salt,
            settings.MinHashLength,
            settings.Alphabet,
            settings.Separators);
    }

    /// <inheritdoc />
    public long? Encode(int number)
    {
        if (number == 0)
        {
            return 0;
        }

        if (number < 0)
        {
            return null;
        }

        var hash = _hashids.Encode(number);
        if (string.IsNullOrEmpty(hash))
        {
            return null;
        }

        var sb = new StringBuilder(hash.Length * 2);
        foreach (var c in hash)
        {
            var code = (int)c - AsciiOffset;
            sb.Append(code.ToString("D2"));
        }

        return long.TryParse(sb.ToString(), out var result) ? result : null;
    }

    /// <inheritdoc />
    public int? Decode(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            return null;
        }

        if (number == "0")
        {
            return 0;
        }

        if (number.Length < 2 || number.Length % 2 != 0)
        {
            return null;
        }

        foreach (var c in number)
        {
            if (!char.IsDigit(c))
            {
                return null;
            }
        }

        var sb = new StringBuilder(number.Length / 2);
        for (var i = 0; i < number.Length; i += 2)
        {
            var chunk = number.Substring(i, 2);
            if (!int.TryParse(chunk, out var code))
            {
                return null;
            }

            code += AsciiOffset;
            if (code is < 0 or > 127)
            {
                return null;
            }

            sb.Append((char)code);
        }

        var hash = sb.ToString();
        var decoded = _hashids.Decode(hash);

        if (decoded.Length != 1)
        {
            return null;
        }

        if (decoded[0] > int.MaxValue || decoded[0] < 0)
        {
            return null;
        }

        return (int)decoded[0];
    }
}
