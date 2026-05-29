using simple_api.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();
app.MapApiEndpoints();

app.Run();

public partial class Program;
