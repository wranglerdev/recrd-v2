namespace Recrd.Domain.Entities;

/// <summary>
/// Massa de testes: conjunto de variáveis nomeadas (PRD §7). Ex.: usuario->admin.
/// Os valores podem ser editados e a massa renomeada na tela Massas.
/// </summary>
public sealed class Massa : AuditableEntity
{
    public required string Name { get; set; }

    // ponytail: 1 linha de valores por massa (PRD §7).
    public Dictionary<string, string> Variables { get; init; } = [];

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
}
