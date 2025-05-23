
using System.Reflection;
using HotChocolate.Types.Descriptors;
using Microsoft.Extensions.Options;
using SigdnRhStaggingApi.Settings;

namespace SigdnRhStaggingApi.Graphql;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class UseWriteApiKeyMiddlewareAttribute : ObjectFieldDescriptorAttribute
{
    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor.Use(next => async ctx =>
        {
            var options = ctx.Services.GetRequiredService<IOptions<AppSettingsOptions>>();
            await new ApiKeyMiddleware(next, options, ApiKeyAccess.WRITE).InvokeAsync(ctx);
        });
    }
}