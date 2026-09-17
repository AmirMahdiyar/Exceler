using Exceler.DependencyInjection;
using OpenXmlSampleWebApi.Profiles;
using OpenXmlSampleWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add API Controllers
builder.Services.AddControllers();

// Configure Exceler v2.0.0 with the OpenXML SAX Engine!
// Notice: OpenXML is MIT-licensed, so NO EPPlus license call is required!
builder.Services.AddExcelCore(options =>
{
    options.UseOpenXmlEngine();
    options.RegisterFromAssemblyContaining<OrderExportProfile>();
});

// Register Bogus synthetic data generator
builder.Services.AddSingleton<OrderDataGenerator>();

// Add Swagger / OpenAPI documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Exceler v2.0.0 - OpenXML SAX Engine Sample & Stress API",
        Version = "v2.0.0",
        Description = "Demonstrates high-performance O(1) memory spreadsheet streaming powered by Exceler's OpenXML SAX engine and Bogus synthetic data."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exceler v2.0.0 OpenXML API");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
