using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence;

public class FlatRateDbContext(DbContextOptions<FlatRateDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations will be added here
    }
}
