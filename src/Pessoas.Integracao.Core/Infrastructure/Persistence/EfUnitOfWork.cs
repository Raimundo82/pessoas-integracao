using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Infrastructure.Data;

namespace Pessoas.Integracao.Core.Infrastructure.Persistence;

public class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    public Task CommitAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);

}
