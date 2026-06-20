using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Recrd.Infrastructure.Data;

/// <summary>
/// Usado só pelas ferramentas <c>dotnet ef</c> (migrations). Em runtime o
/// contexto é configurado pela DI com o caminho real em AppData (PRD §4).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Glue de design-time das ferramentas EF.")]
public sealed class RecrdDbContextFactory : IDesignTimeDbContextFactory<RecrdDbContext>
{
    public RecrdDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RecrdDbContext>()
            .UseSqlite("Data Source=recrd.db")
            .Options;
        return new RecrdDbContext(options);
    }
}
