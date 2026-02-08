using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var providerName = configuration["Database:Provider"] ?? "Sqlite";
        var provider = Enum.Parse<DatabaseProvider>(providerName, ignoreCase: true);

        services.AddDbContext<ContractIntelDbContext>(options =>
        {
            switch (provider)
            {
                case DatabaseProvider.Sqlite:
                    var sqliteConnection = configuration.GetConnectionString("Sqlite")
                        ?? "Data Source=contractintel.db";
                    options.UseSqlite(sqliteConnection);
                    break;

                case DatabaseProvider.SqlServer:
                    var sqlServerConnection = configuration.GetConnectionString("SqlServer")
                        ?? throw new InvalidOperationException("SqlServer connection string is required");
                    // Note: Install Microsoft.EntityFrameworkCore.SqlServer package to use SQL Server
                    throw new NotSupportedException("SqlServer provider requires Microsoft.EntityFrameworkCore.SqlServer package");

                case DatabaseProvider.PostgreSQL:
                    var postgresConnection = configuration.GetConnectionString("PostgreSQL")
                        ?? throw new InvalidOperationException("PostgreSQL connection string is required");
                    // Note: Install Npgsql.EntityFrameworkCore.PostgreSQL package to use PostgreSQL
                    throw new NotSupportedException("PostgreSQL provider requires Npgsql.EntityFrameworkCore.PostgreSQL package");

                default:
                    throw new NotSupportedException($"Database provider {provider} is not supported");
            }

            var enableSensitiveLogging = configuration["Database:EnableSensitiveDataLogging"] == "true";
            if (enableSensitiveLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Register repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IContractRepository, ContractRepository>();

        // Register file storage service
        services.AddSingleton<IFileStorageService>(sp =>
        {
            var storageRoot = configuration["Storage:RootPath"] ?? "storage";
            var logger = sp.GetRequiredService<ILogger<LocalFileStorageService>>();
            return new LocalFileStorageService(storageRoot, logger);
        });

        // Register PDF text extraction service
        services.AddScoped<IPdfTextExtractionService, PdfPigTextExtractionService>();

        return services;
    }
}
