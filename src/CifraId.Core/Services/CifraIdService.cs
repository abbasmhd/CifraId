using CifraId.Encoding;

namespace CifraId.Services;

/// <summary>
/// Default implementation of <see cref="ICifraIdService"/> that delegates
/// to an <see cref="IEncoder"/> for obfuscation.
/// </summary>
public sealed class CifraIdService : ICifraIdService
{
    private readonly IEncoder _encoder;

    /// <summary>Creates a new <see cref="CifraIdService"/>.</summary>
    public CifraIdService(IEncoder encoder)
    {
        _encoder = encoder ?? throw new ArgumentNullException(nameof(encoder));
    }

    /// <inheritdoc />
    public string? EncodeId(int id)
    {
        var encoded = _encoder.Encode(id);
        return encoded?.ToString();
    }

    /// <inheritdoc />
    public int? DecodeId(string? encodedId) => _encoder.Decode(encodedId);

    /// <inheritdoc />
    public string?[] EncodeIds(params int[] ids) =>
        ids.Select(EncodeId).ToArray();

    /// <inheritdoc />
    public int?[] DecodeIds(params string[] encodedIds) =>
        encodedIds.Select(id => DecodeId(id)).ToArray();

    /// <inheritdoc />
    public string? EncodeEnum<TEnum>(TEnum enumValue) where TEnum : struct, Enum =>
        EncodeId(Convert.ToInt32(enumValue));

    /// <inheritdoc />
    public TEnum? DecodeEnum<TEnum>(string? encodedEnum) where TEnum : struct, Enum
    {
        var decoded = DecodeId(encodedEnum);
        if (decoded is null) return null;
        return Enum.IsDefined(typeof(TEnum), decoded.Value)
            ? (TEnum)Enum.ToObject(typeof(TEnum), decoded.Value)
            : null;
    }

    /// <inheritdoc />
    public string?[] EncodeEnums<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum =>
        enumValues.Select(EncodeEnum).ToArray();

    /// <inheritdoc />
    public TEnum?[] DecodeEnums<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum =>
        encodedEnums.Select(DecodeEnum<TEnum>).ToArray();
}
