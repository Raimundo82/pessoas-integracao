using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Services;
using SigdnRhStaggingApi.Settings;
using SigdnRhStaggingApi.Graphql.Queries;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
services.AddDbContextFactory<RhStaggingDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddScoped<IEmployeeService, EmployeeService>()
        .AddAuthorization()
        .AddControllers();


// GraphQL service
services.AddGraphQLServer()
        .RegisterDbContextFactory<RhStaggingDbContext>()
        .AddAuthorization()
        .ModifyRequestOptions(options => options.IncludeExceptionDetails = true)
        .AddQueryType<EmployeeQuery>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RhStaggingDbContext>();
    await db.Database.MigrateAsync();
}

app.UsePathBase(configuration.GetSection(AppSettingsOptions.AppSettings).Get<AppSettingsOptions>()?.SubPath)
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
