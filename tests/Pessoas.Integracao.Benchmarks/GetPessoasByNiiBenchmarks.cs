using BenchmarkDotNet.Attributes;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;

namespace Pessoas.Integracao.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class GetPessoasByNiiBenchmarks
{
    [Params(500, 5_000, 50_000)]
    public int N;

    private BenchmarkDatabase _db = null!;
    private AppDbContext _context = null!;
    private IPessoaRepository _repo = null!;
    private IReadOnlyList<string> _niis = null!;
    private readonly CancellationToken _ct = CancellationToken.None;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _db = new BenchmarkDatabase();
        await _db.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repo = new PessoaRepository(_context);

        var pessoas = PessoaDataFactory.CreatePessoas(N);
        _context.Pessoas.AddRange(pessoas);
        await _context.SaveChangesAsync(_ct);
        _context.ChangeTracker.Clear();

        _niis = pessoas.Select(p => p.NII).ToList();
    }

    [Benchmark]
    public Task<IReadOnlyList<Pessoa>> GetPessoasByNiiAsync() =>
        _repo.GetPessoasByNiiAsync(_niis, _ct);

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _context.DisposeAsync();
        await _db.DisposeAsync();
    }
}
