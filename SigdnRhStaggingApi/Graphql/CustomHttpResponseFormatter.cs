using System.Net;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;

namespace SigdnRhStaggingApi.Graphql;

public class CustomHttpResponseFormatter : DefaultHttpResponseFormatter
{
    protected override HttpStatusCode OnDetermineStatusCode(IOperationResult result, FormatInfo format, HttpStatusCode? proposedStatusCode)
    {
        if (result.Errors?.Count > 0 && result.Errors.Any(error => error.Code == nameof(ErrorCodes.AUTH_NOT_AUTHENTICATED)))
        {
            return HttpStatusCode.Unauthorized;
        }
        return base.OnDetermineStatusCode(result, format, proposedStatusCode);
    }
}