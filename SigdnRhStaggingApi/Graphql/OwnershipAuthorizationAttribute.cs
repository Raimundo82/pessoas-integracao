using System.Reflection;
using HotChocolate.Types.Descriptors;

namespace SigdnRhStaggingApi.Graphql;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class OwnershipAuthorizationAttribute(string argumentName) : ObjectFieldDescriptorAttribute
{
    private readonly string _argumentName = argumentName;

    protected override void OnConfigure(IDescriptorContext context, IObjectFieldDescriptor descriptor, MemberInfo member)
    {
        descriptor.Use(next => async ctx =>
        {
            var httpContextAcessor = ctx.Services.GetRequiredService<IHttpContextAccessor>();

            await new OwnershipAuthorizationMiddleware(next, httpContextAcessor, _argumentName).InvokeAsync(ctx);
        });
    }

}