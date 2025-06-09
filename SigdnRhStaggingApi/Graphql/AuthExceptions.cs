namespace SigdnRhStaggingApi.Graphql;

public enum ErrorCodes
{
    AUTH_NOT_AUTHENTICATED,
    FORBIDDEN
}

public static class AuthError
{
    public static IError Unauthorized(string message) => CreateError(message, nameof(ErrorCodes.AUTH_NOT_AUTHENTICATED));
    public static IError Forbidden(string message) => CreateError(message, nameof(ErrorCodes.FORBIDDEN));

    private static IError CreateError(string message, string code) => ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .Build();

}

public abstract class AuthException(IError error) : GraphQLException(error);

public class UnauthorizedException(string message = "Unauthorized") : AuthException(AuthError.Unauthorized(message));

public class ForbiddenException(string message = "Forbidden") : AuthException(AuthError.Forbidden(message));