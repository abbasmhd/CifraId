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
    public int? DecodeId(string? encodedId)
    {
        if (string.IsNullOrWhiteSpace(encodedId)) return null;
        return int.TryParse(encodedId, out var result) ? result : null;
    }

    /// <inheritdoc />
    public string?[] EncodeIds(params int[] ids) =>
        ids.Select(EncodeId).ToArray();

    /// <inheritdoc />
    public int?[] DecodeIds(params string[] encodedIds) =>
        encodedIds.Select(id => DecodeId(id)).ToArray();

    /// <inheritdoc />
    public string? EncodeEnum<TEnum>(TEnum enumValue) where TEnum : struct, Enum =>
        Convert.ToInt32(enumValue).ToString();

    /// <inheritdoc />
    public TEnum? DecodeEnum<TEnum>(string? encodedEnum) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(encodedEnum)) return null;
        if (!int.TryParse(encodedEnum, out var intValue)) return null;
        return Enum.IsDefined(typeof(TEnum), intValue)
            ? (TEnum)Enum.ToObject(typeof(TEnum), intValue)
            : null;
    }

    /// <inheritdoc />
    public string?[] EncodeEnums<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum =>
        enumValues.Select(EncodeEnum).ToArray();

    /// <inheritdoc />
    public TEnum?[] DecodeEnums<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum =>
        encodedEnums.Select(DecodeEnum<TEnum>).ToArray();
}
