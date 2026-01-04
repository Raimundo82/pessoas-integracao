using Pessoas.Integracao.Core.Application;
using Pessoas.Integracao.Core.Infrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.Shared.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddOpenApi()
    .AddPersistence(builder.Configuration)
    .AddRepositories()
    .AddUseCases()
    .AddExternalSoapClientServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options => options.DocumentPath = "/openapi/v1.json");
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();