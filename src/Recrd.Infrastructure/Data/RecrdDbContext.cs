using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Recrd.Domain.Entities;

namespace Recrd.Infrastructure.Data;

/// <summary>
/// Contexto EF Core/SQLite local-first (PRD §3, §4, §6). A hierarquia
/// Project>TestPlan>TestSuite>TestCase é descoberta por convenção (FKs).
/// </summary>
public sealed class RecrdDbContext(DbContextOptions<RecrdDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TestPlan> TestPlans => Set<TestPlan>();
    public DbSet<TestSuite> TestSuites => Set<TestSuite>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<Execution> Executions => Set<Execution>();
    public DbSet<Massa> Massas => Set<Massa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ManualScript é a representação intermediária — guardada como JSON numa coluna (PRD §6).
        var comparer = new ValueComparer<List<ScriptAction>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.GetHashCode())),
            v => v.ToList());

        modelBuilder.Entity<TestCase>()
            .Property(c => c.ManualScript)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<ScriptAction>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(comparer);

        // Massa.Variables (PRD §7) também vira JSON numa coluna.
        var varsComparer = new ValueComparer<Dictionary<string, string>>(
            (a, b) => a!.SequenceEqual(b!),
            v => v.Aggregate(0, (h, x) => HashCode.Combine(h, x.Key.GetHashCode(), x.Value.GetHashCode())),
            v => new Dictionary<string, string>(v));

        modelBuilder.Entity<Massa>()
            .Property(m => m.Variables)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new())
            .Metadata.SetValueComparer(varsComparer);
    }
}
