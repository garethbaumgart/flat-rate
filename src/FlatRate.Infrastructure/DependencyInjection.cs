using FlatRate.Domain.Aggregates.Bills;
using FlatRate.Domain.Aggregates.Properties;
using FlatRate.Domain.Aggregates.Users;
using FlatRate.Domain.Repositories;
using FlatRate.Infrastructure.Persistence;
using FlatRate.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FlatRate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<FlatRateDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IPropertyAccessRepository, PropertyAccessRepository>();
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
