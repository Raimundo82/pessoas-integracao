using Quartz;

namespace Pessoas.Integracao.Worker.Infrastructure.Jobs;

public class ImportAndApplyExternalPessoaChangesJob(ILogger<ImportAndApplyExternalPessoaChangesJob> logger) : IJob
{
    public static readonly JobKey Key = new("ImportAndApplyExternalPessoaChangesJob", "PessoasIntegration");

    public Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Importing and Applying Pessoas changes daily on Week day at midnight");
        return Task.CompletedTask;
    }
}