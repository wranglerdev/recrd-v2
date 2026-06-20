namespace Recrd.Domain.Entities;

/// <summary>Plano de teste de um projeto (PRD §6).</summary>
public sealed class TestPlan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public List<TestSuite> Suites { get; } = [];
}
