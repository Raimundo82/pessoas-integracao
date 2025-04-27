using SigdnRhStaggingApi.Settings;
using SigdnRhStaggingApi.Startup;
using SigdnRhStaggingApi.Middleware;
using SigdnRhStaggingApi;
using HotChocolate.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services.AddInfrastructure(configuration)
        .AddAuth(configuration)
        .AddAuthorization()
        .AddAppServices()
        .AddGraphQl()
        .AddControllers();

var app = builder.Build();

DatabaseUtils.MigrateDatabase(app.Services);

app.UsePathBase(configuration.GetSection(AppSettingsOptions.AppSettings).Get<AppSettingsOptions>()?.SubPath)
    .UseRequestLogging()
    .UseAuthentication()
    .UseAuthorization();

app.MapGraphQL()
    .WithOptions(new GraphQLServerOptions { Tool = { ServeMode = GraphQLToolServeMode.Embedded } });

await app.RunAsync();
