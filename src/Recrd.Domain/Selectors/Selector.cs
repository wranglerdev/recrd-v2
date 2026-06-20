namespace Recrd.Domain.Selectors;

/// <summary>Estratégias em ordem de prioridade (PRD §11).</summary>
public enum SelectorStrategy
{
    TestId,
    AriaLabel,
    Id,
    Name,
    Role,
    Text,
    Css,
    XPath
}

/// <summary>
/// Seletor gerado para um elemento. <see cref="IsStable"/> = false dispara
/// o alerta de "seletor instável" na UI (PRD §11).
/// </summary>
public sealed record Selector(string Value, SelectorStrategy Strategy, bool IsStable);

/// <summary>Atributos capturados de um elemento durante a gravação (PRD §10).</summary>
public sealed record ElementInfo(
    string Tag,
    string? TestId = null,
    string? AriaLabel = null,
    string? Id = null,
    string? Name = null,
    string? Role = null,
    string? Text = null,
    string? Css = null,
    string? XPath = null);
