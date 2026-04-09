using System.Text.Json;
using Microsoft.Extensions.Options;
using CifraId.Configuration;
using CifraId.Encoding;
using CifraId.Json;
using CifraId.Services;
using CifraId.Transforms;

namespace CifraId.Tests;

public class JsonConverterTests
{
    private readonly ICifraIdService _service;
    private readonly JsonSerializerOptions _options;

    public JsonConverterTests()
    {
        var settings = Options.Create(new HashSettings
        {
            Salt = "json-test-salt",
            MinHashLength = 6,
        });
        var encoder = new Encoder(settings);
        _service = new CifraIdService(encoder);
        var transform = new DefaultCifraIdOutboundStringTransform();
        var factory = new CifraIdJsonConverterFactory(_service, transform);

        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { factory },
        };
    }

    [Fact]
    public void Serialize_Encodes_CifraId_Int_Property()
    {
        var dto = new TestDto { Id = 42, Name = "Test" };
        var json = JsonSerializer.Serialize(dto, _options);
        var doc = JsonDocument.Parse(json);

        var idValue = doc.RootElement.GetProperty("id").GetString();
        Assert.NotNull(idValue);
        Assert.NotEqual("42", idValue);

        var nameValue = doc.RootElement.GetProperty("name").GetString();
        Assert.Equal("Test", nameValue);
    }

    [Fact]
    public void Deserialize_Decodes_CifraId_Int_Property()
    {
        var original = new TestDto { Id = 42, Name = "Test" };
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json, _options);

        Assert.NotNull(deserialized);
        Assert.Equal(42, deserialized!.Id);
        Assert.Equal("Test", deserialized.Name);
    }

    [Fact]
    public void RoundTrip_Multiple_Values()
    {
        var original = new TestDto
        {
            Id = 123,
            Name = "Widget",
            ParentId = 456,
            Secret = 999,
            CustomNamedId = 789,
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json, _options)!;

        Assert.Equal(123, deserialized.Id);
        Assert.Equal("Widget", deserialized.Name);
        Assert.Equal(456, deserialized.ParentId);
        Assert.Equal(0, deserialized.Secret); // JsonIgnore -> not serialized -> default
        Assert.Equal(789, deserialized.CustomNamedId);
    }

    [Fact]
    public void Serialize_NullableInt_Null_Writes_Null()
    {
        var dto = new TestDto { Id = 1, ParentId = null };
        var json = JsonSerializer.Serialize(dto, _options);
        var doc = JsonDocument.Parse(json);

        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("parentId").ValueKind);
    }

    [Fact]
    public void RoundTrip_NullableInt_WithValue()
    {
        var dto = new TestDto { Id = 1, ParentId = 99 };
        var json = JsonSerializer.Serialize(dto, _options);
        var deserialized = JsonSerializer.Deserialize<TestDto>(json, _options)!;

        Assert.Equal(99, deserialized.ParentId);
    }

    [Fact]
    public void Serialize_Respects_JsonIgnore()
    {
        var dto = new TestDto { Id = 1, Secret = 42 };
        var json = JsonSerializer.Serialize(dto, _options);
        var doc = JsonDocument.Parse(json);

        Assert.False(doc.RootElement.TryGetProperty("secret", out _));
        Assert.False(doc.RootElement.TryGetProperty("Secret", out _));
    }

    [Fact]
    public void Serialize_Uses_Custom_JsonPropertyName_From_Attribute()
    {
        var dto = new TestDto { Id = 1, CustomNamedId = 42 };
        var json = JsonSerializer.Serialize(dto, _options);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("customId", out var prop));
        Assert.Equal(JsonValueKind.String, prop.ValueKind);
    }

    [Fact]
    public void Deserialize_Invalid_Encoded_String_NonNullable_Gets_Default()
    {
        var idEncoded = _service.EncodeId(1)!;
        var json = $"{{\"id\":\"{idEncoded}\",\"parentId\":\"invalid\",\"name\":\"Test\",\"customId\":\"invalid\"}}";
        var deserialized = JsonSerializer.Deserialize<TestDto>(json, _options)!;

        Assert.Equal(1, deserialized.Id);
        Assert.Null(deserialized.ParentId);
        Assert.Equal(0, deserialized.CustomNamedId); // non-nullable default
    }

    [Fact]
    public void Serialize_Enum_Property()
    {
        var dto = new TestDtoWithEnum { Status = TestStatus.Active };
        var json = JsonSerializer.Serialize(dto, _options);
        var doc = JsonDocument.Parse(json);

        var statusValue = doc.RootElement.GetProperty("status").GetString();
        Assert.NotNull(statusValue);
        Assert.NotEqual("1", statusValue);
    }

    [Fact]
    public void RoundTrip_Enum_Properties()
    {
        var original = new TestDtoWithEnum
        {
            Status = TestStatus.Active,
            NullableStatus = TestStatus.Archived,
        };
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<TestDtoWithEnum>(json, _options)!;

        Assert.Equal(TestStatus.Active, deserialized.Status);
        Assert.Equal(TestStatus.Archived, deserialized.NullableStatus);
    }

    [Fact]
    public void RoundTrip_NullableEnum_Null()
    {
        var original = new TestDtoWithEnum
        {
            Status = TestStatus.Active,
            NullableStatus = null,
        };
        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<TestDtoWithEnum>(json, _options)!;

        Assert.Equal(TestStatus.Active, deserialized.Status);
        Assert.Null(deserialized.NullableStatus);
    }

    [Fact]
    public void CamelCase_NamingPolicy_Applied()
    {
        var dto = new TestDto { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(dto, _options);

        Assert.Contains("\"id\"", json);
        Assert.Contains("\"name\"", json);
        Assert.Contains("\"parentId\"", json);
    }

    [Fact]
    public void Serialize_Null_Object_Writes_Null()
    {
        TestDto? dto = null;
        var json = JsonSerializer.Serialize(dto, _options);
        Assert.Equal("null", json);
    }

    [Fact]
    public void Deserialize_Null_Json_Returns_Null()
    {
        var result = JsonSerializer.Deserialize<TestDto>("null", _options);
        Assert.Null(result);
    }
}
