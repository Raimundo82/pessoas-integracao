namespace SigdnRhStaggingApi.Graphql;

public enum ErrorCodes
{
    AUTH_NOT_AUTHENTICATED,
}

public static class GraphQLErrorHelper
{
    public static IError Unauthorized(string message = "Unauthorized") => CreateError(message, nameof(ErrorCodes.AUTH_NOT_AUTHENTICATED));

    private static IError CreateError(string message, string code) => ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .Build();

}