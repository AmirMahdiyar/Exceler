using Exceler.DependencyInjection;
using TestExcelerWebApi.Profile;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddExcelCore(options =>
{
    options.RegisterFromAssemblyContaining<EmployeeExcelProfile>();
}); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
