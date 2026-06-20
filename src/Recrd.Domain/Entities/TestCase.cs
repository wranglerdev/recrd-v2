namespace Recrd.Domain.Entities;

public enum TestCaseStatus
{
    Draft,
    Ready,
    Passing,
    Failing
}

/// <summary>
/// Caso de teste com seu script manual (ações gravadas), o script Robot
/// compilado e o histórico de execuções (PRD §6).
/// </summary>
public sealed class TestCase : AuditableEntity
{
    public Guid TestSuiteId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public TestCaseStatus Status { get; set; } = TestCaseStatus.Draft;

    /// <summary>Ações gravadas — representação intermediária (PRD §6, §13).</summary>
    public List<ScriptAction> ManualScript { get; } = [];

    /// <summary>Saída Robot Framework + Playwright após compilar (PRD §13).</summary>
    public string? CompiledScript { get; set; }

    public List<Execution> Executions { get; } = [];
}
