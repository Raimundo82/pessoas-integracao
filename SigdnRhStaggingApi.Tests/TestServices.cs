using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Graphql.Mutations;
using SigdnRhStaggingApi.Graphql.Queries;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Tests;

public class TestServices
{
    public IServiceProvider Services { get; }

    public RequestExecutorProxy Executor { get; }
    public TestServices(string dbName)
    {
        Services = new ServiceCollection()
            .AddLogging()
            .AddAuthorization()
            .AddDbContextFactory<RhStaggingDbContext>(options => options.UseInMemoryDatabase(dbName))
            .AddScoped<IEmployeeService, EmployeeService>()
            .AddGraphQLServer()
            .AddMutationConventions(applyToAllMutations: true)
            .AddAuthorization()
            .AddQueryType<EmployeeQuery>()
            .AddMutationType<EmployeeMutation>()
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
