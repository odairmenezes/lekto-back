using CadPlus.Data;
using Microsoft.EntityFrameworkCore;

namespace CadPlus.Tests.TestUtilities;

/// <summary>
/// Fixture para testes que precisam de banco de dados
/// </summary>
public class DatabaseFixture : IDisposable
{
    private const string ConnectionString = "DataSource=:memory:";
    private readonly DbContextOptions<CadPlusDbContext> _options;

    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<CadPlusDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CadPlusDbContext(_options);
        context.Database.EnsureCreated();
    }

    public CadPlusDbContext CreateContext()
    {
        return new CadPlusDbContext(_options);
    }

    public void Dispose()
    {
        using var context = new CadPlusDbContext(_options);
        context.Database.EnsureDeleted();
    }
}
