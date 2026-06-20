namespace Recrd.Domain.Entities;

/// <summary>
/// Massa de testes: conjunto de variáveis nomeadas (PRD §7). Ex.: usuario->admin.
/// Os valores podem ser editados e a massa renomeada na tela Massas.
/// </summary>
public sealed class Massa
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }

    // ponytail: 1 linha de valores por massa (PRD §7). Histórico completo de
    // importações é auditoria (issue separada) — aqui guardamos só ImportedAt.
    public Dictionary<string, string> Variables { get; init; } = [];

    public DateTimeOffset ImportedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; set; }
}
