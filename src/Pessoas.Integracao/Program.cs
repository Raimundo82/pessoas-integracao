using Pessoas.Integracao;
using Pessoas.Integracao.Infrastructure;
using Pessoas.Integracao.Infrastructure.Data;
var builder = Host.CreateApplicationBuilder(args);
builder.AddInfrastructureServices();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.InitialiseDatabaseAsync();
await host.RunAsync();