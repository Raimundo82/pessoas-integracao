using SigdnRhStaggingApi.Graphql.Mutations;
using SigdnRhStaggingApi.Graphql.Queries;

namespace SigdnRhStaggingApi.Startup;

public static class GraphqlServiceExtensions
{
    public static IServiceCollection AddGraphQl(this IServiceCollection services)
    {
        services.AddGraphQLServer()
        .AddMutationConventions(applyToAllMutations: true)
        .AddAuthorization()
        .ModifyRequestOptions(options => options.IncludeExceptionDetails = true)
        .AddQueryType<EmployeeQuery>()
        .AddMutationType<EmployeeMutation>()
        .AddFiltering()
        .AddSorting()
        .AddProjections();

        return services;
    }

}