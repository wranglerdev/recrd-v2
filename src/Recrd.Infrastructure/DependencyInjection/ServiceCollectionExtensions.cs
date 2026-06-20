using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Recrd.Application.Abstractions;
using Recrd.Infrastructure.Auth;
using Recrd.Infrastructure.Data;
using Recrd.Infrastructure.Storage;
using Serilog;

namespace Recrd.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra storage local-first, SQLite, identidade e logging Serilog (PRD §3, §31).
    /// Fail fast: cria a árvore de diretórios já no registro.
    /// </summary>
    public static IServiceCollection AddRecrdInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Storage (PRD §4). Root opcional via config; default = %LOCALAPPDATA%/recrd.
        var paths = new RecrdPaths(configuration["Recrd:StorageRoot"]);
        paths.EnsureCreated(); // fail fast se não der p/ escrever
        services.AddSingleton(paths);

        // Logging estruturado em logs/app.log (PRD §4, §31).
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(paths.LogsDir, "app.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        services.AddLogging(b => b.AddSerilog(logger, dispose: true));

        // Identidade: Windows em produção, Mock no dev Linux (PRD §5, §29).
        services.AddSingleton<IUserContext>(_ =>
            OperatingSystem.IsWindows() ? new WindowsUserContext() : new MockUserContext());

        services.AddDbContext<RecrdDbContext>(o => o.UseSqlite($"Data Source={paths.DatabaseFile}"));

        return services;
    }
}
