namespace CifraId.Services;

/// <summary>
/// Pass-through implementation of <see cref="ICifraIdService"/> used
/// when encoding is disabled in development mode.
/// Values are returned as-is without obfuscation.
/// </summary>
public sealed class NoOpCifraIdService : ICifraIdService
{
    /// <inheritdoc />
    public string? EncodeId(int id) => id.ToString();

    /// <inheritdoc />
    public string? EncodeId(long id) => id.ToString();

    /// <inheritdoc />
    public Task<string?> EncodeIdAsync(int id) => Task.FromResult(EncodeId(id));

    /// <inheritdoc />
    public Task<string?> EncodeIdAsync(long id) => Task.FromResult(EncodeId(id));

    /// <inheritdoc />
    public int? DecodeId(string? encodedId)
    {
        if (string.IsNullOrWhiteSpace(encodedId))
        {
            return null;
        }

        return int.TryParse(encodedId, out var result) ? result : null;
    }

    /// <inheritdoc />
    public long? DecodeIdLong(string? encodedId)
    {
        if (string.IsNullOrWhiteSpace(encodedId))
        {
            return null;
        }

        return long.TryParse(encodedId, out var result) ? result : null;
    }

    /// <inheritdoc />
    public Task<int?> DecodeIdAsync(string? encodedId) => Task.FromResult(DecodeId(encodedId));

    /// <inheritdoc />
    public Task<long?> DecodeIdLongAsync(string? encodedId) => Task.FromResult(DecodeIdLong(encodedId));

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
        Convert.ToInt32(enumValue).ToString();

    /// <inheritdoc />
    public Task<string?> EncodeEnumAsync<TEnum>(TEnum enumValue) where TEnum : struct, Enum =>
        Task.FromResult(EncodeEnum(enumValue));

    /// <inheritdoc />
    public TEnum? DecodeEnum<TEnum>(string? encodedEnum) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(encodedEnum))
        {
            return null;
        }

        if (!int.TryParse(encodedEnum, out var intValue))
        {
            return null;
        }

        return Enum.IsDefined(typeof(TEnum), intValue)
            ? (TEnum)Enum.ToObject(typeof(TEnum), intValue)
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
