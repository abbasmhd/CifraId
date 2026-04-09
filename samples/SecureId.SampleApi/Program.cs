using CifraId.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register all CifraId services, JSON converters, and model binder in one call.
builder.Services.AddCifraId(
    builder.Configuration,
    builder.Environment.IsDevelopment());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CifraId Sample API v1");
    });
}

app.MapControllers();

app.Run();
