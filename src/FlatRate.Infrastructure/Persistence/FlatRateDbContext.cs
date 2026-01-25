using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;

namespace FlatRate.Infrastructure.Persistence;

public class FlatRateDbContext(DbContextOptions<FlatRateDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PropertyAccess> PropertyAccess => Set<PropertyAccess>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlatRateDbContext).Assembly);
    }
}
