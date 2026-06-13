using Microsoft.Extensions.Logging;

namespace CifraId.Services;

/// <summary>
/// A decorator for <see cref="ICifraIdService"/> that adds logging for encoding/decoding operations.
/// </summary>
public sealed class LoggingCifraIdService : ICifraIdService
{
    private readonly ICifraIdService _inner;
    private readonly ILogger<LoggingCifraIdService> _logger;

    /// <summary>Creates a new <see cref="LoggingCifraIdService"/>.</summary>
    public LoggingCifraIdService(ICifraIdService inner, ILogger<LoggingCifraIdService> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string? EncodeId(int id)
    {
        var result = _inner.EncodeId(id);
        if (result is null)
        {
            _logger.LogWarning("Failed to encode int ID: {Id}", id);
        }
        return result;
    }

    /// <inheritdoc />
    public string? EncodeId(long id)
    {
        var result = _inner.EncodeId(id);
        if (result is null)
        {
            _logger.LogWarning("Failed to encode long ID: {Id}", id);
        }
        return result;
    }

    /// <inheritdoc />
    public Task<string?> EncodeIdAsync(int id) =>
        Task.FromResult(EncodeId(id));

    /// <inheritdoc />
    public Task<string?> EncodeIdAsync(long id) =>
        Task.FromResult(EncodeId(id));

    /// <inheritdoc />
    public int? DecodeId(string? encodedId)
    {
        if (string.IsNullOrWhiteSpace(encodedId))
        {
            return null;
        }

        var result = _inner.DecodeId(encodedId);
        if (result is null)
        {
            _logger.LogWarning("Failed to decode ID: {EncodedId}", encodedId);
        }
        return result;
    }

    /// <inheritdoc />
    public long? DecodeIdLong(string? encodedId)
    {
        if (string.IsNullOrWhiteSpace(encodedId))
        {
            return null;
        }

        var result = _inner.DecodeIdLong(encodedId);
        if (result is null)
        {
            _logger.LogWarning("Failed to decode ID to long: {EncodedId}", encodedId);
        }
        return result;
    }

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
        if (string.IsNullOrWhiteSpace(encodedEnum))
        {
            return null;
        }

        var result = _inner.DecodeEnum<TEnum>(encodedEnum);
        if (result is null)
        {
            _logger.LogWarning("Failed to decode enum value: {EncodedEnum}", encodedEnum);
        }
        return result;
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
