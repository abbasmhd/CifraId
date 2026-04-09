using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using SecureId.Configuration;
using SecureId.Encoding;
using SecureId.ModelBinding;
using SecureId.Services;

namespace SecureId.Tests;

public class ModelBinderTests
{
    private readonly ISecureIdService _service;
    private readonly SecureIdModelBinder _binder = new();

    public ModelBinderTests()
    {
        var settings = Options.Create(new HashSettings
        {
            Salt = "binder-test-salt",
            MinHashLength = 6,
        });
        var encoder = new Encoder(settings);
        _service = new SecureIdService(encoder);
    }

    [Fact]
    public async Task Binds_SecureId_Property_From_PascalCase()
    {
        var encodedId = _service.EncodeId(42)!;
        var ctx = CreateContext<QueryModel>(new Dictionary<string, StringValues>
        {
            ["Id"] = encodedId,
            ["Name"] = "Test",
        });

        await _binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet);
        var model = Assert.IsType<QueryModel>(ctx.Result.Model);
        Assert.Equal(42, model.Id);
        Assert.Equal("Test", model.Name);
    }

    [Fact]
    public async Task Binds_SecureId_Property_From_CamelCase()
    {
        var encodedId = _service.EncodeId(42)!;
        var ctx = CreateContext<QueryModel>(new Dictionary<string, StringValues>
        {
            ["id"] = encodedId,
        });

        await _binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet);
        var model = Assert.IsType<QueryModel>(ctx.Result.Model);
        Assert.Equal(42, model.Id);
    }

    [Fact]
    public async Task Invalid_SecureId_Value_Adds_ModelState_Error()
    {
        var ctx = CreateContext<QueryModel>(new Dictionary<string, StringValues>
        {
            ["Id"] = "invalid_encoded",
        });

        await _binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet);
        Assert.False(ctx.ModelState.IsValid);
        Assert.True(ctx.ModelState.ContainsKey("Id"));
    }

    [Fact]
    public async Task Binds_NullableSecureId_Property()
    {
        var encodedId = _service.EncodeId(99)!;
        var encodedStatus = _service.EncodeEnum(TestStatus.Active)!;
        var ctx = CreateContext<QueryModelWithNullable>(new Dictionary<string, StringValues>
        {
            ["OrderId"] = encodedId,
            ["Status"] = encodedStatus,
        });

        await _binder.BindModelAsync(ctx);

        Assert.True(ctx.Result.IsModelSet);
        var model = Assert.IsType<QueryModelWithNullable>(ctx.Result.Model);
        Assert.Equal(99, model.OrderId);
        Assert.Equal(TestStatus.Active, model.Status);
    }

    [Fact]
    public void Provider_Returns_Binder_For_SecureId_Model()
    {
        var provider = new SecureIdModelBinderProvider();
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(typeof(QueryModel));

        var context = new TestModelBinderProviderContext(metadata);
        var binder = provider.GetBinder(context);

        Assert.NotNull(binder);
        Assert.IsType<SecureIdModelBinder>(binder);
    }

    [Fact]
    public void Provider_Returns_Null_For_NonSecureId_Model()
    {
        var provider = new SecureIdModelBinderProvider();
        var metadataProvider = new EmptyModelMetadataProvider();
        var metadata = metadataProvider.GetMetadataForType(typeof(PlainModel));

        var context = new TestModelBinderProviderContext(metadata);
        var binder = provider.GetBinder(context);

        Assert.Null(binder);
    }

    private ModelBindingContext CreateContext<T>(Dictionary<string, StringValues> queryValues)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_service);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

        var valueProvider = new QueryStringValueProvider(
            BindingSource.Query,
            new QueryCollection(queryValues),
            System.Globalization.CultureInfo.InvariantCulture);

        return new DefaultModelBindingContext
        {
            ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(T)),
            ModelName = string.Empty,
            ValueProvider = valueProvider,
            ActionContext = new ActionContext { HttpContext = httpContext },
            ModelState = new ModelStateDictionary(),
        };
    }
}

public class PlainModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

/// <summary>
/// Minimal provider context stub for testing <see cref="SecureIdModelBinderProvider"/>.
/// </summary>
internal sealed class TestModelBinderProviderContext : ModelBinderProviderContext
{
    public TestModelBinderProviderContext(ModelMetadata metadata) => Metadata = metadata;

    public override ModelMetadata Metadata { get; }
    public override BindingInfo BindingInfo => new();
    public override IModelMetadataProvider MetadataProvider => new EmptyModelMetadataProvider();

    public override IModelBinder CreateBinder(ModelMetadata metadata) =>
        throw new NotSupportedException();
}
