namespace Recrd.Domain.Entities;

// ponytail: minimal action set from PRD §10/§13. Add kinds (Hover, Select, Upload…)
// when the recorder actually emits them.
public enum ActionKind
{
    Click,
    Input,
    Navigate,
    Wait,
    Assert
}

/// <summary>
/// Uma ação do script manual (PRD §6 exemplo JSON). <see cref="Value"/> guarda
/// templates de massa como <c>{{usuario}}</c>, não o valor resolvido (PRD §12).
/// </summary>
public sealed record ScriptAction(ActionKind Action, string? Selector = null, string? Value = null);
