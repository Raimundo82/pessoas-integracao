using SigdnRhStaggingApi.Settings;
using SigdnRhStaggingApi.Startup;
using SigdnRhStaggingApi.Middleware;
using SigdnRhStaggingApi;
using HotChocolate.AspNetCore;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.
services
    .Configure<AppSettingsOptions>(configuration.GetSection(AppSettingsOptions.AppSettings))
    .AddInfrastructure(configuration)
    .AddAuth(configuration)
    .AddAuthorization()
    .AddAppServices()
    .AddGraphQl()
    .AddControllers();

var app = builder.Build();

DatabaseUtils.MigrateDatabase(app.Services);

var appSettings = app.Services.GetRequiredService<IOptions<AppSettingsOptions>>().Value;
app.UsePathBase(appSettings.SubPath)
    .UseRequestLogging()
    .UseAuthentication()
    .UseAuthorization();

app.MapGraphQL()
    .WithOptions(new GraphQLServerOptions { Tool = { ServeMode = GraphQLToolServeMode.Embedded } });

await app.RunAsync();