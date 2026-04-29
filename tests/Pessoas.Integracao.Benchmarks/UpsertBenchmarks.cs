using BenchmarkDotNet.Attributes;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;

namespace Pessoas.Integracao.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 3)]
public class UpsertBenchmarks
{
    [Params(500, 5_000, 50_000)]
    public int N;
    private BenchmarkDatabase _db = null!;
    private DbContextOptions<AppDbContext> _dbOptions = null!;
    private AppDbContext _context = null!;
    private IPessoaRepository _repo = null!;
    private IReadOnlyList<Pessoa> _pessoas = null!;
    private readonly CancellationToken _ct = CancellationToken.None;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        _db = new BenchmarkDatabase();
        await _db.StartAsync();
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_db.ConnectionString)
            .Options;
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _context?.Dispose();
        _context = new AppDbContext(_dbOptions);
        _repo = new PessoaRepository(_context);

        _context.Pessoas.ExecuteDeleteAsync(_ct).GetAwaiter().GetResult();

        var toSeed = PessoaDataFactory.CreatePessoas(N);
        _context.Pessoas.AddRange(toSeed);
        _context.SaveChangesAsync(_ct).GetAwaiter().GetResult();
        _context.ChangeTracker.Clear();
        _pessoas = PessoaDataFactory.CreatePessoas(N);
    }

    [Benchmark]
    public async Task UpsertAllAsync()
    {
        await _repo.UpsertAllAsync(_pessoas, _ct);
        await _context.SaveChangesAsync(_ct);
    }

    [Benchmark]
    public Task BulkUpsertAsync() => _repo.BulkUpsertAsync(_pessoas, _ct);

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await _context.DisposeAsync();
        await _db.DisposeAsync();
    }
}
