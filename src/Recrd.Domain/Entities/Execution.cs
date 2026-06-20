namespace Recrd.Domain.Entities;

public enum ExecutionResult
{
    Passed,
    Failed
}

/// <summary>Registro de uma execução de caso de teste (PRD §6, §15).</summary>
public sealed class Execution
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TestCaseId { get; set; }
    public DateTimeOffset ExecutedAt { get; set; } = DateTimeOffset.UtcNow;
    public ExecutionResult Result { get; set; }

    /// <summary>Usuário que disparou a execução (PRD §5, §15).</summary>
    public string? User { get; set; }

    public string? Log { get; set; }
    public TimeSpan Duration { get; set; }
}
