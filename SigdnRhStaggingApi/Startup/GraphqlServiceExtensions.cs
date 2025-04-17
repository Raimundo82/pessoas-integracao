using System;
using SigdnRhStaggingApi.Graphql.Queries;

namespace SigdnRhStaggingApi.Startup;

public static class GraphqlServiceExtensions
{
    public static IServiceCollection AddGraphQl(this IServiceCollection services)
    {
        services.AddGraphQLServer()
        .AddAuthorization()
        .ModifyRequestOptions(options => options.IncludeExceptionDetails = true)
        .AddQueryType<EmployeeQuery>();

        return services;
    }

}
