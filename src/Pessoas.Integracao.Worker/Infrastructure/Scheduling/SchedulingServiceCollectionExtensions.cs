using Pessoas.Integracao.Worker.Infrastructure.Jobs;

using Quartz;

namespace Pessoas.Integracao.Worker.Infrastructure.Scheduling;

public static class SchedulingServiceCollectionExtensions
{
    public static IServiceCollection AddSchedulingServices(this IServiceCollection services)
    {
        return services.AddQuartz(q =>
        {
            q.AddJob<ImportAndApplyExternalPessoaChangesJob>(opts =>
                opts.WithIdentity(ImportAndApplyExternalPessoaChangesJob.Key)
            );

            q.AddTrigger(opts => opts
                .ForJob(ImportAndApplyExternalPessoaChangesJob.Key)
                .WithIdentity("WeekDayDailyTrigger", "PessoasIntegration")
                .WithCronSchedule(
                    CronScheduleBuilder
                        .AtHourAndMinuteOnGivenDaysOfWeek(
                            0,
                            0,
                            DayOfWeek.Monday,
                            DayOfWeek.Tuesday,
                            DayOfWeek.Wednesday,
                            DayOfWeek.Thursday,
                            DayOfWeek.Friday
                        )
                    )
            );
        })
        .AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);
    }
}