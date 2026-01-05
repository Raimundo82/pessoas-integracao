using Pessoas.Integracao.Core.Infrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Extensions;
using Pessoas.Integracao.Worker.Infrastructure.Scheduling;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPersistence(builder.Configuration)
    .AddRepositories()
    .AddExternalSoapClientServices(builder.Configuration)
    .AddSchedulingServices();

var host = builder.Build();

if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
{
    await host.InitialiseDatabaseAsync();
}

await host.RunAsync();