using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Settings;
using SigdnRhStaggingApi.Startup;

namespace SigdnRhStaggingApi.Tests.GraphqlTesting;

public class GraphqlTestServices
{
    public IServiceProvider Services { get; }
    public RequestExecutorProxy Executor { get; }
    public GraphqlTestServices()
    {
        Guid uniqueId = Guid.NewGuid();

        Services = new ServiceCollection()
            .AddLogging()
            .Configure<AppSettingsOptions>(options => options.AllowMissingHttpContext = true)
            .AddAuthorization()
            .AddDbContextFactory<RhStaggingDbContext>(options => options.UseInMemoryDatabase(uniqueId.ToString()))
            .AddHttpContextAccessor()
            .AddAppServices()
            .AddGraphQLServer()
            .AddMutationConventions(applyToAllMutations: true)
            .AddAuthorization()
            .ModifyRequestOptions(options => options.IncludeExceptionDetails = true)
            .AddTypes()
            .AddFiltering()
            .AddSorting()
            .AddProjections()
            .Services
            .AddSingleton(sp => new RequestExecutorProxy(sp.GetRequiredService<IRequestExecutorResolver>(), Schema.DefaultName))
            .BuildServiceProvider();

        Executor = Services.GetRequiredService<RequestExecutorProxy>();
    }


    public async Task<(IServiceScope scope, RhStaggingDbContext dbContext)> CreateScopeAndDbContextAsync()
    {
        var dbScope = Services.CreateAsyncScope();
        var dbFactory = dbScope.ServiceProvider.GetRequiredService<IDbContextFactory<RhStaggingDbContext>>();
        var dbContext = await dbFactory.CreateDbContextAsync();
        return (dbScope, dbContext);
    }

    public async Task<string> ExecuteRequestAsync(
        Action<OperationRequestBuilder> configureRequest,
        CancellationToken cancellationToken = default)
    {
        var requestBuilder = new OperationRequestBuilder();
        requestBuilder.SetServices(Services.CreateAsyncScope().ServiceProvider);
        configureRequest(requestBuilder);
        var request = requestBuilder.Build();

        await using var result = await Executor.ExecuteAsync(request, cancellationToken);

        result.ExpectOperationResult();

        return result.ToJson();
    }

}