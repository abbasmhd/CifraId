namespace SecureId.Services;

/// <summary>
/// High-level service for encoding and decoding integer and enum values
/// to and from SecureId obfuscated strings.
/// </summary>
public interface ISecureIdService
{
    /// <summary>Encodes an integer ID into an obfuscated string.</summary>
    string? EncodeId(int id);

    /// <summary>Decodes an obfuscated string back into the original integer ID.</summary>
    int? DecodeId(string? encodedId);

    /// <summary>Encodes multiple integer IDs.</summary>
    string?[] EncodeIds(params int[] ids);

    /// <summary>Decodes multiple obfuscated strings.</summary>
    int?[] DecodeIds(params string[] encodedIds);

    /// <summary>Encodes an enum value into an obfuscated string via its underlying integer.</summary>
    string? EncodeEnum<TEnum>(TEnum enumValue) where TEnum : struct, Enum;

    /// <summary>Decodes an obfuscated string back into an enum value.</summary>
    TEnum? DecodeEnum<TEnum>(string? encodedEnum) where TEnum : struct, Enum;

    /// <summary>Encodes multiple enum values.</summary>
    string?[] EncodeEnums<TEnum>(params TEnum[] enumValues) where TEnum : struct, Enum;

    /// <summary>Decodes multiple obfuscated strings back into enum values.</summary>
    TEnum?[] DecodeEnums<TEnum>(params string[] encodedEnums) where TEnum : struct, Enum;
}
