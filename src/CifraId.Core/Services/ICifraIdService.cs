namespace CifraId.Services;

/// <summary>
/// High-level service for encoding and decoding integer and enum values
/// to and from CifraId obfuscated strings.
/// </summary>
public interface ICifraIdService
{
    /// <summary>Encodes an integer ID into an obfuscated string.</summary>
    string? EncodeId(int id);

    /// <summary>Encodes a long ID into an obfuscated string.</summary>
    string? EncodeId(long id);

    /// <summary>Asynchronously encodes an integer ID into an obfuscated string.</summary>
    Task<string?> EncodeIdAsync(int id);

    /// <summary>Asynchronously encodes a long ID into an obfuscated string.</summary>
    Task<string?> EncodeIdAsync(long id);

    /// <summary>Decodes an obfuscated string back into the original integer ID.</summary>
    int? DecodeId(string? encodedId);

    /// <summary>Decodes an obfuscated string back into the original long ID.</summary>
    long? DecodeIdLong(string? encodedId);

    /// <summary>Asynchronously decodes an obfuscated string back into the original integer ID.</summary>
    Task<int?> DecodeIdAsync(string? encodedId);

    /// <summary>Asynchronously decodes an obfuscated string back into the original long ID.</summary>
    Task<long?> DecodeIdLongAsync(string? encodedId);

    /// <summary>Encodes multiple integer IDs.</summary>
    string?[] EncodeIds(params int[] ids);

    /// <summary>Encodes multiple long IDs.</summary>
    string?[] EncodeIds(params long[] ids);

    /// <summary>Asynchronously encodes multiple integer IDs.</summary>
    Task<string?[]> EncodeIdsAsync(params int[] ids);

    /// <summary>Asynchronously encodes multiple long IDs.</summary>
    Task<string?[]> EncodeIdsAsync(params long[] ids);

    /// <summary>Decodes multiple obfuscated strings.</summary>
    int?[] DecodeIds(params string[] encodedIds);

    /// <summary>Decodes multiple obfuscated strings to long.</summary>
    long?[] DecodeIdsLong(params string[] encodedIds);

    /// <summary>Asynchronously decodes multiple obfuscated strings.</summary>
    Task<int?[]> DecodeIdsAsync(params string[] encodedIds);

    /// <summary>Asynchronously decodes multiple obfuscated strings to long.</summary>
    Task<long?[]> DecodeIdsLongAsync(params string[] encodedIds);

    /// <summary>Encodes an enum value into an obfuscated string via its underlying integer.</summary>
    string? EncodeEnum<TEnum>(TEnum enumValue) where TEnum : struct, Enum;

    /// <summary>Asynchronously encodes an enum value into an obfuscated string.</summary>
    Task<string?> EncodeEnumAsync<TEnum>(TEnum enumValue) where TEnum : struct, Enum;

    /// <summary>Decodes an obfuscated string back into an enum value.</summary>
    TEnum? DecodeEnum<TEnum>(string? encodedEnum) where TEnum : struct, Enum;

    /// <summary>Asynchronously decodes an obfuscated string back into an enum value.</summary>
    Task<TEnum?> DecodeEnumAsync<TEnum>(string? encodedEnum) where TEnum : struct, Enum;

    /// <summary>Encodes multiple enum values.</summary>
    string?[] EncodeEnums<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum;

    /// <summary>Asynchronously encodes multiple enum values.</summary>
    Task<string?[]> EncodeEnumsAsync<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum;

    /// <summary>Decodes multiple obfuscated strings back into enum values.</summary>
    TEnum?[] DecodeEnums<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum;

    /// <summary>Asynchronously decodes multiple obfuscated strings back into enum values.</summary>
    Task<TEnum?[]> DecodeEnumsAsync<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum;
}
