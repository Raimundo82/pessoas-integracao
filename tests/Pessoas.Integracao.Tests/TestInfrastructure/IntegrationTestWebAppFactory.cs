using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

using Moq;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Tests.TestInfrastructure;

public class IntegrationTestWebAppFactory(PostgresTestContainerDb dbContainer) : WebApplicationFactory<Program>
{
    private readonly PostgresTestContainerDb _dbContainer = dbContainer;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.ConnectionString));

            var soapDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISoapChannelProvider<zhr_wsChannel>));

            if (soapDescriptor != null)
            {
                services.Remove(soapDescriptor);
            }

            var mockChannelFactory = new Mock<ISoapChannelProvider<zhr_wsChannel>>();

            services.AddSingleton(mockChannelFactory);
            services.AddSingleton(mockChannelFactory.Object);

            var pessoasDataProviderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPessoasDataProvider));

            if (pessoasDataProviderDescriptor != null)
            {
                services.Remove(pessoasDataProviderDescriptor);
            }

            var mockPessoasDataProvider = new Mock<IPessoasDataProvider>();

            services.AddSingleton(mockPessoasDataProvider);
            services.AddSingleton(mockPessoasDataProvider.Object);

            services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });
        });
    }

    public HttpClient CreateAuthenticatedClient(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.NameIdentifier, "test-user-id")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsData = claims.Select(c => new { c.Type, c.Value }).ToList();
        var claimsJson = JsonSerializer.Serialize(claimsData);

        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Claims", claimsJson);
        return client;
    }
}
