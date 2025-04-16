using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Services;
using SigdnRhStaggingApi.Settings;
using SigdnRhStaggingApi.Graphql.Queries;
using Keycloak.AuthServices.Authentication;
using NSwag.Generation.Processors.Security;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag.AspNetCore;
using HotChocolate.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

var keycloakOptions = configuration.GetKeycloakOptions<KeycloakAuthenticationOptions>();

services.AddKeycloakWebApiAuthentication(configuration, options =>
{
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Add services to the container.
services.AddDbContext<RhStaggingDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddScoped<IEmployeeService, EmployeeService>()
        .AddAuthorization()
        .AddControllers();


// GraphQL service
services.AddGraphQLServer()
        .AddAuthorization()
        .ModifyRequestOptions(options => options.IncludeExceptionDetails = true)
        .AddQueryType<EmployeeQuery>();


services.AddOpenApiDocument(options =>
{
    options.AddSecurity("Implicit OAuth2", [], new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
        Description = "Authentication",
        Name = "SIGDN RH Stagging API",
        Flow = NSwag.OpenApiOAuth2Flow.Implicit,
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
    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Implicit OAuth2"));

    options.AddSecurity("JWT Bearer", [], new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Description = "Type into the textbox: {your JWT token}."
    });
    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT Bearer"));

});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RhStaggingDbContext>();
    await db.Database.MigrateAsync();
}

app.UsePathBase(configuration.GetSection(AppSettingsOptions.AppSettings).Get<AppSettingsOptions>()?.SubPath)
    .UseOpenApi()
    .UseSwaggerUi(settings => settings.OAuth2Client = new OAuth2ClientSettings
    {
        ClientId = keycloakOptions?.Resource,
        ClientSecret = keycloakOptions?.Credentials.Secret,
        AppName = keycloakOptions?.Resource,
        Realm = keycloakOptions?.Realm,
    })
   .UseAuthentication()
   .UseAuthorization();

app.MapGraphQL()
    .WithOptions(
        new GraphQLServerOptions
        {
            Tool = { ServeMode = GraphQLToolServeMode.Embedded }
        }
    );

app.MapControllers();

await app.RunAsync();
