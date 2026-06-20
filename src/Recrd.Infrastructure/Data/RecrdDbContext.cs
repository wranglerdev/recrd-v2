using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Recrd.Application.Abstractions;
using Recrd.Domain.Entities;

namespace Recrd.Infrastructure.Data;

/// <summary>
/// Contexto EF Core/SQLite local-first (PRD §3, §4, §6). A hierarquia
/// Project>TestPlan>TestSuite>TestCase é descoberta por convenção (FKs).
/// Carimba auditoria (PRD §16) usando <see cref="IUserContext"/>.
/// </summary>
public sealed class RecrdDbContext(DbContextOptions<RecrdDbContext> options, IUserContext? user = null)
    : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TestPlan> TestPlans => Set<TestPlan>();
    public DbSet<TestSuite> TestSuites => Set<TestSuite>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<Execution> Executions => Set<Execution>();
    public DbSet<Massa> Massas => Set<Massa>();

    public override int SaveChanges()
    {
        StampAudit();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAudit();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAudit()
    {
        var now = DateTimeOffset.UtcNow;
        var who = user?.Username;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = who;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = who;
            }
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Mapeamento declarativo do EF, não lógica de negócio.")]
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
