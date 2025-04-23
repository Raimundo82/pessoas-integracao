using HotChocolate;
using HotChocolate.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Graphql.Queries;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Tests;

public static class TestServices
{
    static TestServices()
    {
        Services = new ServiceCollection()
            .AddLogging()
            .AddAuthorization()
            .AddDbContextFactory<RhStaggingDbContext>(options => options.UseInMemoryDatabase("TestInMemDB"))
            .AddScoped<IEmployeeService, EmployeeService>()
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<EmployeeQuery>()
            .Services
            .AddSingleton(sp => new RequestExecutorProxy(sp.GetRequiredService<IRequestExecutorResolver>(), Schema.DefaultName))
            .BuildServiceProvider();

        Executor = Services.GetRequiredService<RequestExecutorProxy>();
    }

    public static IServiceProvider Services { get; }

    public static RequestExecutorProxy Executor { get; }

    public static async Task<(IServiceScope scope, RhStaggingDbContext dbContext)> CreateScopeAndDbContextAsync()
    {
        var scope = Services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var dbFactory = provider.GetRequiredService<IDbContextFactory<RhStaggingDbContext>>();
        var dbContext = await dbFactory.CreateDbContextAsync();

        return (scope, dbContext);
    }

    public static async Task<string> ExecuteRequestAsync(
        IServiceProvider serviceProvider,
        Action<OperationRequestBuilder> configureRequest,
        CancellationToken cancellationToken = default)
    {
        var requestBuilder = new OperationRequestBuilder();
        requestBuilder.SetServices(serviceProvider);
        configureRequest(requestBuilder);
        var request = requestBuilder.Build();

        await using var result = await Executor.ExecuteAsync(request, cancellationToken);

        result.ExpectOperationResult();

        return result.ToJson();
    }

}
