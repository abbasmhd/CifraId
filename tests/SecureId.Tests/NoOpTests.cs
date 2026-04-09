using SecureId.Encoding;
using SecureId.Services;

namespace SecureId.Tests;

public class NoOpTests
{
    [Fact]
    public void NoOpEncoder_Encode_Returns_SameValue()
    {
        var encoder = new NoOpEncoder();
        Assert.Equal(123L, encoder.Encode(123));
    }

    [Fact]
    public void NoOpEncoder_Decode_Returns_SameValue()
    {
        var encoder = new NoOpEncoder();
        Assert.Equal(123, encoder.Decode("123"));
    }

    [Fact]
    public void NoOpEncoder_Decode_Invalid_Returns_Null()
    {
        var encoder = new NoOpEncoder();
        Assert.Null(encoder.Decode("not_a_number"));
    }

    [Fact]
    public void NoOpEncoder_Decode_Null_Returns_Null()
    {
        var encoder = new NoOpEncoder();
        Assert.Null(encoder.Decode(null));
    }

    [Fact]
    public void NoOpEncoder_Decode_Empty_Returns_Null()
    {
        var encoder = new NoOpEncoder();
        Assert.Null(encoder.Decode(""));
    }

    [Fact]
    public void NoOpService_EncodeId_Returns_PlainString()
    {
        var service = new NoOpSecureIdService();
        Assert.Equal("42", service.EncodeId(42));
    }

    [Fact]
    public void NoOpService_DecodeId_Parses_String()
    {
        var service = new NoOpSecureIdService();
        Assert.Equal(42, service.DecodeId("42"));
    }

    [Fact]
    public void NoOpService_DecodeId_Null_Returns_Null()
    {
        var service = new NoOpSecureIdService();
        Assert.Null(service.DecodeId(null));
    }

    [Fact]
    public void NoOpService_RoundTrip()
    {
        var service = new NoOpSecureIdService();
        var encoded = service.EncodeId(42);
        var decoded = service.DecodeId(encoded);
        Assert.Equal(42, decoded);
    }

    [Fact]
    public void NoOpService_Enum_RoundTrip()
    {
        var service = new NoOpSecureIdService();
        var encoded = service.EncodeEnum(TestStatus.Active);
        var decoded = service.DecodeEnum<TestStatus>(encoded);
        Assert.Equal(TestStatus.Active, decoded);
    }

    [Fact]
    public void NoOpService_Batch_RoundTrip()
    {
        var service = new NoOpSecureIdService();
        var ids = new[] { 1, 2, 3 };
        var encoded = service.EncodeIds(ids);
        Assert.All(encoded, e => Assert.NotNull(e));

        var decoded = service.DecodeIds(encoded.Select(e => e!).ToArray());
        Assert.Equal(ids, decoded.Select(d => d!.Value).ToArray());
    }
}
