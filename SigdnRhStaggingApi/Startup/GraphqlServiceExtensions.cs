namespace SigdnRhStaggingApi.Startup;

public static class GraphqlServiceExtensions
{
    public static IServiceCollection AddGraphQl(this IServiceCollection services)
    {
        services.AddGraphQLServer()
        .AddTypes()
        .AddMutationConventions(applyToAllMutations: true)
        .AddAuthorization()
        .ModifyRequestOptions(options => options.IncludeExceptionDetails = true)
        .AddFiltering()
        .AddSorting()
        .AddProjections();

        return services;
    }

}