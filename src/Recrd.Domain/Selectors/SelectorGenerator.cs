namespace Recrd.Domain.Selectors;

/// <summary>
/// Escolhe o melhor seletor seguindo a prioridade do PRD §11 e marca
/// confiança. Nunca emite XPath absoluto.
/// </summary>
public static class SelectorGenerator
{
    public static Selector Generate(ElementInfo el)
    {
        if (!string.IsNullOrWhiteSpace(el.TestId))
            return new($"[data-testid=\"{el.TestId}\"]", SelectorStrategy.TestId, IsStable: true);

        if (!string.IsNullOrWhiteSpace(el.AriaLabel))
            return new($"[aria-label=\"{el.AriaLabel}\"]", SelectorStrategy.AriaLabel, IsStable: true);

        if (!string.IsNullOrWhiteSpace(el.Id))
            return new($"#{el.Id}", SelectorStrategy.Id, IsStable: true);

        if (!string.IsNullOrWhiteSpace(el.Name))
            return new($"[name=\"{el.Name}\"]", SelectorStrategy.Name, IsStable: true);

        if (!string.IsNullOrWhiteSpace(el.Role))
            return new($"{el.Tag}[role=\"{el.Role}\"]", SelectorStrategy.Role, IsStable: true);

        if (!string.IsNullOrWhiteSpace(el.Text))
            return new($"text={el.Text}", SelectorStrategy.Text, IsStable: true);

        if (!string.IsNullOrWhiteSpace(el.Css))
            return new(el.Css, SelectorStrategy.Css, IsStable: IsStableCss(el.Css));

        if (!string.IsNullOrWhiteSpace(el.XPath))
        {
            if (el.XPath.StartsWith('/') && !el.XPath.StartsWith("//"))
                throw new ArgumentException("XPath absoluto não é permitido (PRD §11).", nameof(el));
            // XPath é sempre o último recurso — baixa confiança.
            return new(el.XPath, SelectorStrategy.XPath, IsStable: false);
        }

        throw new InvalidOperationException("Elemento sem nenhum atributo selecionável.");
    }

    // ponytail: heurística simples — seletores posicionais são frágeis (PRD §11 exemplo
    // div:nth-child(5)). Refinar (profundidade do encadeamento, classes voláteis) se necessário.
    private static bool IsStableCss(string css) =>
        !css.Contains("nth-child", StringComparison.OrdinalIgnoreCase)
        && !css.Contains("nth-of-type", StringComparison.OrdinalIgnoreCase);
}
