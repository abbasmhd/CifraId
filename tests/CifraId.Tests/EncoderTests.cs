using Microsoft.Extensions.Options;
using CifraId.Configuration;
using CifraId.Encoding;

namespace CifraId.Tests;

public class EncoderTests
{
    private readonly Encoder _encoder;

    public EncoderTests()
    {
        var settings = Options.Create(new HashSettings
        {
            Salt = "test-salt",
            MinHashLength = 6,
            Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890",
            Separators = "cfhistuCFHISTU",
        });
        _encoder = new Encoder(settings);
    }

    [Fact]
    public void Encode_Zero_Returns_Zero()
    {
        Assert.Equal(0L, _encoder.Encode(0));
    }

    [Fact]
    public void Decode_Zero_Returns_Zero()
    {
        Assert.Equal(0, _encoder.Decode("0"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(12345)]
    [InlineData(999999)]
    [InlineData(int.MaxValue)]
    public void Encode_Decode_RoundTrip(int value)
    {
        var encoded = _encoder.Encode(value);
        Assert.NotNull(encoded);

        var decoded = _encoder.Decode(encoded!.Value.ToString());
        Assert.Equal(value, decoded);
    }

    [Fact]
    public void Encode_Negative_Returns_Null()
    {
        Assert.Null(_encoder.Encode(-1));
    }

    [Fact]
    public void Decode_Null_Returns_Null()
    {
        Assert.Null(_encoder.Decode(null));
    }

    [Fact]
    public void Decode_Empty_Returns_Null()
    {
        Assert.Null(_encoder.Decode(""));
    }

    [Fact]
    public void Decode_Whitespace_Returns_Null()
    {
        Assert.Null(_encoder.Decode("   "));
    }

    [Fact]
    public void Decode_NonNumeric_Returns_Null()
    {
        Assert.Null(_encoder.Decode("abc"));
    }

    [Fact]
    public void Decode_OddLength_Returns_Null()
    {
        Assert.Null(_encoder.Decode("123"));
    }

    [Fact]
    public void Decode_Malformed_Returns_Null()
    {
        Assert.Null(_encoder.Decode("9999"));
    }

    [Fact]
    public void Different_Salts_Produce_Different_Encodings()
    {
        var settings1 = Options.Create(new HashSettings { Salt = "salt-one", MinHashLength = 6 });
        var settings2 = Options.Create(new HashSettings { Salt = "salt-two", MinHashLength = 6 });

        var encoder1 = new Encoder(settings1);
        var encoder2 = new Encoder(settings2);

        var encoded1 = encoder1.Encode(42);
        var encoded2 = encoder2.Encode(42);

        Assert.NotEqual(encoded1, encoded2);
    }

    [Fact]
    public void Encoded_Value_Contains_Only_Digits()
    {
        var encoded = _encoder.Encode(42);
        Assert.NotNull(encoded);

        var str = encoded!.Value.ToString();
        Assert.All(str, c => Assert.True(char.IsDigit(c)));
    }

    [Fact]
    public void Encoded_Value_Is_Not_Same_As_Input()
    {
        var encoded = _encoder.Encode(42);
        Assert.NotNull(encoded);
        Assert.NotEqual(42L, encoded);
    }

    [Fact]
    public void Multiple_Encodes_Are_Deterministic()
    {
        var first = _encoder.Encode(42);
        var second = _encoder.Encode(42);
        Assert.Equal(first, second);
    }
}
