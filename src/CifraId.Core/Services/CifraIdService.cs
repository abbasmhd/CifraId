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
    public string? EncodeId(long id)
    {
        var encoded = _encoder.Encode(id);
        return encoded?.ToString();
    }

    /// <inheritdoc />
    public Task<string?> EncodeIdAsync(int id) =>
        Task.FromResult(EncodeId(id));

    /// <inheritdoc />
    public Task<string?> EncodeIdAsync(long id) =>
        Task.FromResult(EncodeId(id));

    /// <inheritdoc />
    public int? DecodeId(string? encodedId) => _encoder.Decode(encodedId);

    /// <inheritdoc />
    public long? DecodeIdLong(string? encodedId) => _encoder.DecodeLong(encodedId);

    /// <inheritdoc />
    public Task<int?> DecodeIdAsync(string? encodedId) =>
        Task.FromResult(DecodeId(encodedId));

    /// <inheritdoc />
    public Task<long?> DecodeIdLongAsync(string? encodedId) =>
        Task.FromResult(DecodeIdLong(encodedId));

    /// <inheritdoc />
    public string?[] EncodeIds(params int[] ids) =>
        ids.Select(EncodeId).ToArray();

    /// <inheritdoc />
    public string?[] EncodeIds(params long[] ids) =>
        ids.Select(EncodeId).ToArray();

    /// <inheritdoc />
    public Task<string?[]> EncodeIdsAsync(params int[] ids) =>
        Task.FromResult(EncodeIds(ids));

    /// <inheritdoc />
    public Task<string?[]> EncodeIdsAsync(params long[] ids) =>
        Task.FromResult(EncodeIds(ids));

    /// <inheritdoc />
    public int?[] DecodeIds(params string[] encodedIds) =>
        encodedIds.Select(DecodeId).ToArray();

    /// <inheritdoc />
    public long?[] DecodeIdsLong(params string[] encodedIds) =>
        encodedIds.Select(DecodeIdLong).ToArray();

    /// <inheritdoc />
    public Task<int?[]> DecodeIdsAsync(params string[] encodedIds) =>
        Task.FromResult(DecodeIds(encodedIds));

    /// <inheritdoc />
    public Task<long?[]> DecodeIdsLongAsync(params string[] encodedIds) =>
        Task.FromResult(DecodeIdsLong(encodedIds));

    /// <inheritdoc />
    public string? EncodeEnum<TEnum>(TEnum enumValue) where TEnum : struct, Enum =>
        EncodeId(Convert.ToInt32(enumValue));

    /// <inheritdoc />
    public Task<string?> EncodeEnumAsync<TEnum>(TEnum enumValue) where TEnum : struct, Enum =>
        Task.FromResult(EncodeEnum(enumValue));

    /// <inheritdoc />
    public TEnum? DecodeEnum<TEnum>(string? encodedEnum) where TEnum : struct, Enum
    {
        var decoded = DecodeId(encodedEnum);
        if (decoded is null)
        {
            return null;
        }

        return Enum.IsDefined(typeof(TEnum), decoded.Value)
            ? (TEnum)Enum.ToObject(typeof(TEnum), decoded.Value)
            : null;
    }

    /// <inheritdoc />
    public Task<TEnum?> DecodeEnumAsync<TEnum>(string? encodedEnum) where TEnum : struct, Enum =>
        Task.FromResult(DecodeEnum<TEnum>(encodedEnum));

    /// <inheritdoc />
    public string?[] EncodeEnums<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum =>
        enumValues.Select(EncodeEnum).ToArray();

    /// <inheritdoc />
    public Task<string?[]> EncodeEnumsAsync<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum =>
        Task.FromResult(EncodeEnums(enumValues));

    /// <inheritdoc />
    public TEnum?[] DecodeEnums<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum =>
        encodedEnums.Select(DecodeEnum<TEnum>).ToArray();

    /// <inheritdoc />
    public Task<TEnum?[]> DecodeEnumsAsync<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum =>
        Task.FromResult(DecodeEnums<TEnum>(encodedEnums));
}
