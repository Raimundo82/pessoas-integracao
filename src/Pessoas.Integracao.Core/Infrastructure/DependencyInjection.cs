using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Interfaces;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Persistence;
using Pessoas.Integracao.Core.Infrastructure.Repositories;

namespace Pessoas.Integracao.Core.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<DbInitialiser>();
        services.AddScoped<IPessoaRepository, PessoaRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ImportAllPessoas>();
        return services;
    }

}