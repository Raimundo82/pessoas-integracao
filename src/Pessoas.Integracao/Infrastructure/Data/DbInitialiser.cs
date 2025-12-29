namespace Pessoas.Integracao.Infrastructure.Data;

public class DbInitialiser(AppDbContext context, ILogger<DbInitialiser> logger)
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<DbInitialiser> _logger = logger;


    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

}