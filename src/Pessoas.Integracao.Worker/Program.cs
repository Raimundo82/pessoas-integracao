using Pessoas.Integracao.Core.Infrastructure;
using Pessoas.Integracao.Worker;
using Pessoas.Integracao.Worker.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
{
    await host.InitialiseDatabaseAsync();
}

await host.RunAsync();