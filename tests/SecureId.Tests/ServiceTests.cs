using Microsoft.Extensions.Options;
using SecureId.Configuration;
using SecureId.Encoding;
using SecureId.Services;

namespace SecureId.Tests;

public class ServiceTests
{
    private readonly SecureIdService _service;

    public ServiceTests()
    {
        var settings = Options.Create(new HashSettings
        {
            Salt = "svc-test-salt",
            MinHashLength = 6,
        });
        var encoder = new Encoder(settings);
        _service = new SecureIdService(encoder);
    }

    [Fact]
    public void EncodeId_DecodeId_RoundTrip()
    {
        var encoded = _service.EncodeId(42);
        Assert.NotNull(encoded);

        var decoded = _service.DecodeId(encoded);
        Assert.Equal(42, decoded);
    }

    [Fact]
    public void EncodeId_Zero_Returns_Zero_String()
    {
        Assert.Equal("0", _service.EncodeId(0));
    }

    [Fact]
    public void DecodeId_Null_Returns_Null()
    {
        Assert.Null(_service.DecodeId(null));
    }

    [Fact]
    public void DecodeId_Empty_Returns_Null()
    {
        Assert.Null(_service.DecodeId(""));
    }

    [Fact]
    public void EncodeIds_DecodeIds_RoundTrip()
    {
        var ids = new[] { 1, 2, 3, 42, 100 };
        var encoded = _service.EncodeIds(ids);
        Assert.Equal(ids.Length, encoded.Length);
        Assert.All(encoded, e => Assert.NotNull(e));

        var decoded = _service.DecodeIds(encoded.Select(e => e!).ToArray());
        Assert.Equal(ids, decoded.Select(d => d!.Value).ToArray());
    }

    [Fact]
    public void EncodeEnum_DecodeEnum_RoundTrip()
    {
        var encoded = _service.EncodeEnum(TestStatus.Active);
        Assert.NotNull(encoded);

        var decoded = _service.DecodeEnum<TestStatus>(encoded);
        Assert.Equal(TestStatus.Active, decoded);
    }

    [Fact]
    public void EncodeEnums_DecodeEnums_RoundTrip()
    {
        var values = new[] { TestStatus.Active, TestStatus.Inactive, TestStatus.Archived };
        var encoded = _service.EncodeEnums(values);
        Assert.Equal(3, encoded.Length);
        Assert.All(encoded, e => Assert.NotNull(e));

        var decoded = _service.DecodeEnums<TestStatus>(encoded.Select(e => e!).ToArray());
        Assert.Equal(values, decoded.Select(d => d!.Value).ToArray());
    }

    [Fact]
    public void DecodeEnum_Null_Returns_Null()
    {
        Assert.Null(_service.DecodeEnum<TestStatus>(null));
    }

    [Fact]
    public void DecodeEnum_Invalid_Returns_Null()
    {
        Assert.Null(_service.DecodeEnum<TestStatus>("invalid_value"));
    }

    [Fact]
    public void DecodeEnum_Undefined_IntValue_Returns_Null()
    {
        var encoded = _service.EncodeId(999);
        Assert.NotNull(encoded);
        Assert.Null(_service.DecodeEnum<TestStatus>(encoded));
    }
}
