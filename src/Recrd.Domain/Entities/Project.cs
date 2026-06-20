namespace Recrd.Domain.Entities;

/// <summary>Raiz da hierarquia de automações (PRD §6).</summary>
public sealed class Project
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>Caminho do repositório Robot associado (PRD §14).</summary>
    public string? RobotPath { get; set; }

    public string? CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<TestPlan> Plans { get; } = [];
}
