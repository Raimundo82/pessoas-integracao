using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;

namespace SigdnRhStaggingApi;

public static class DatabaseUtils
{
    public static void Run(RhStaggingDbContext context)
    {
        context.Database.Migrate();
    }

    public static void MigrateDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RhStaggingDbContext>();
        Run(context);
    }
}
