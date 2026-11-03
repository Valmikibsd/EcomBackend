using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Interfaces;
using Product.Application.Services;
using Product.Infrastructure.Repositories;
using SharedKernel.Data;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        const string fallbackConn = "Server=tcp:172.16.0.30,1433;Initial Catalog=saksham;Persist Security Info=False;User ID=dbdeveloper;Password=maha@123;MultipleActiveResultSets=False;Encrypt=false;TrustServerCertificate=False;Connection Timeout=30;";
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = fallbackConn;
        }

        services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IMasterRepository, MasterRepository>();
        services.AddScoped<IMasterService, MasterService>();

        return services;
    }
}
