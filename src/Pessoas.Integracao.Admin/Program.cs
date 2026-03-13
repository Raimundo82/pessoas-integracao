using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;


using Pessoas.Integracao.Admin.Authorization;
using Pessoas.Integracao.Admin.Middleware;
using Pessoas.Integracao.Admin.OpenApi;
using Pessoas.Integracao.Core.Application;
using Pessoas.Integracao.Core.Infrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var keycloakOptions = builder.Configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>() ?? new KeycloakAuthenticationOptions();

builder.Services.AddKeycloakWebApiAuthentication(builder.Configuration);
builder.Services.AddApplicationAuthorization();

builder.Services.AddOpenApiWithAuthentication(keycloakOptions);

builder.Services.AddControllers();

builder.Services
    .AddGlobalExceptionHandling()
    .AddPersistence(builder.Configuration)
    .AddRepositories()
    .AddUseCases()
    .AddExternalSoapClientServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerWithOAuth(keycloakOptions);
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

await app.RunAsync();