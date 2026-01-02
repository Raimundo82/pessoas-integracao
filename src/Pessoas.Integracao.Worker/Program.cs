using Pessoas.Integracao.Core.Infrastructure;
using Pessoas.Integracao.Worker.Infrastructure.Extensions;
using Pessoas.Integracao.Worker.Infrastructure.Scheduling;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCoreServices(builder.Configuration)
    .AddWorkerInfrastuctureServices()
    .AddSchedulingServices()
    .Configure<DataSourceSettings>(builder.Configuration.GetSection(DataSourceSettings.SectionName));

var host = builder.Build();

if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
{
    await host.InitialiseDatabaseAsync();
}

await host.RunAsync();