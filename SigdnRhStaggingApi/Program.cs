using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Services;
using SigdnRhStaggingApi.Settings;
using Keycloak.AuthServices.Authentication;
using NSwag.Generation.Processors.Security;
using Keycloak.AuthServices.Common;
using NSwag.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddKeycloakWebApiAuthentication(configuration);

// Add services to the container.
services.AddDbContext<RhStaggingDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddScoped<IEmployeeService, EmployeeService>()
        .AddAuthorization()
        .AddControllers();

var keycloakOptions = configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>();

services.AddOpenApiDocument(options =>
{
    options.AddSecurity("Bearer", [], new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
        Description = "Authentication",
        Name = "SIGDN RH Stagging API",
        Flows = new NSwag.OpenApiOAuthFlows
        {
            Implicit = new NSwag.OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{keycloakOptions!.KeycloakUrlRealm}protocol/openid-connect/auth").ToString(),
                TokenUrl = new Uri($"{keycloakOptions.KeycloakUrlRealm}protocol/openid-connect/token").ToString(),
                Scopes = new Dictionary<string, string>(),
            },

        },
    });
    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RhStaggingDbContext>();
    await db.Database.MigrateAsync();
}


app.UsePathBase(builder.Configuration.GetSection("ApiSettings").Get<AppSettings>()?.SubPath ?? "/rh-stagging")
   .UseOpenApi()
   .UseSwaggerUi(settings =>
   {
       settings.OAuth2Client = new OAuth2ClientSettings
       {
           ClientId = keycloakOptions?.Resource,
           ClientSecret = keycloakOptions?.Credentials.Secret,
           AppName = keycloakOptions?.Resource,
           Realm = keycloakOptions?.Realm,
       };
   })
   .UseAuthentication()
   .UseAuthorization();

app.MapControllers();

app.Run();
