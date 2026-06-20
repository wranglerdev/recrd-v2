namespace Recrd.Domain.Entities;

/// <summary>Suite de casos dentro de um plano (PRD §6).</summary>
public sealed class TestSuite
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TestPlanId { get; set; }
    public required string Name { get; set; }

    public List<TestCase> Cases { get; } = [];
}
