using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Settings;

namespace SigdnRhStaggingApi.Tests.HttpTesting;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        Guid uniqueId = Guid.NewGuid();
        builder.ConfigureServices(services =>
         {
             services.RemoveAll<IDbContextOptionsConfiguration<RhStaggingDbContext>>();
             services.RemoveAll<DbConnection>();

             services.AddDbContextFactory<RhStaggingDbContext>(options => options.UseInMemoryDatabase(uniqueId.ToString()));

             services.AddSingleton(Options.Create(new AppSettingsOptions
             {
                 ReadApiKey = "read-key",
                 WriteApiKey = "write-key",
                 AllowMissingHttpContext = false,
             }));
         });

    }
}