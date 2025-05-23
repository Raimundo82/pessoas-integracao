using SigdnRhStaggingApi.Settings;
using SigdnRhStaggingApi.Startup;
using SigdnRhStaggingApi.Middleware;
using HotChocolate.AspNetCore;
using Microsoft.Extensions.Options;
using SigdnRhStaggingApi.Graphql;
using SigdnRhStaggingApi;


var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services
    .AddConfig(builder.Configuration)
    .AddHttpContextAccessor()
    .AddInfrastructure(builder.Configuration)
    .AddAuth(builder.Configuration)
    .AddAuthorization()
    .AddAppServices()
    .AddGraphQl()
    .AddHttpResponseFormatter<CustomHttpResponseFormatter>();

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    DatabaseUtils.MigrateDatabase(app.Services);
}

var appSettings = app.Services.GetRequiredService<IOptions<AppSettingsOptions>>().Value;
app.UsePathBase(appSettings.SubPath)
    .UseRequestLogging()
    .UseAuthentication()
    .UseAuthorization();

app.MapGraphQL()
    .WithOptions(new GraphQLServerOptions { Tool = { ServeMode = GraphQLToolServeMode.Embedded } });

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}