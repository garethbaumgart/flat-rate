using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlatRate.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FlatRateDbContext>
{
    public FlatRateDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Environment variable 'ConnectionStrings__DefaultConnection' not set. " +
                "Run migrations inside Docker (docker compose exec api ...) or set the variable manually.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<FlatRateDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new FlatRateDbContext(optionsBuilder.Options);
    }
}
