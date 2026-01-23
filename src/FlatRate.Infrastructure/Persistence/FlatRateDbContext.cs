using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Aggregates.Properties;
using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence;

public class FlatRateDbContext(DbContextOptions<FlatRateDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Bill> Bills => Set<Bill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlatRateDbContext).Assembly);
    }
}
