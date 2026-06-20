namespace Recrd.Domain.Entities;

/// <summary>Suite de casos dentro de um plano (PRD §6).</summary>
public sealed class TestSuite : AuditableEntity
{
    public Guid TestPlanId { get; set; }
    public required string Name { get; set; }

    public List<TestCase> Cases { get; } = [];
}
