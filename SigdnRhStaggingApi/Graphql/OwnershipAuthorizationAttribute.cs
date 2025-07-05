using System.Reflection;
using HotChocolate.Types.Descriptors;
using SigdnRhStaggingApi.Services;

namespace SigdnRhStaggingApi.Graphql;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class OwnershipAuthorizationAttribute(string argumentName) : ObjectFieldDescriptorAttribute
{
    private readonly string _argumentName = argumentName;

    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor.Use(next => async ctx =>
        {
            var currentUserService = ctx.Services.GetRequiredService<ICurrentUserService>();

            await new OwnershipAuthorizationMiddleware(next, currentUserService, _argumentName).InvokeAsync(ctx);
        });
    }

}