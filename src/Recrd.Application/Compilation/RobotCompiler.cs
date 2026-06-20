using System.Text;
using System.Text.RegularExpressions;
using Recrd.Domain.Entities;
using Recrd.Domain.Selectors;

namespace Recrd.Application.Compilation;

public sealed record CompilationResult(string RobotCode, IReadOnlyList<string> Warnings);

/// <summary>
/// Pipeline de compilação (PRD §9, §13): valida ações, analisa seletores,
/// gera Robot Framework + Browser (Playwright) e valida a sintaxe.
/// </summary>
public static class RobotCompiler
{
    // PRD §12: {{var}} -> ${var} (variável Robot, alimentada pela massa).
    private static readonly Regex TemplateVar = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public static CompilationResult Compile(TestCase testCase)
    {
        var actions = testCase.ManualScript;

        // 1. Validação de ações (fail fast, PRD §31).
        var errors = actions.SelectMany(Validate).ToList();
        if (actions.Count == 0)
            errors.Add("Script vazio: nenhuma ação para compilar.");
        if (errors.Count > 0)
            throw new InvalidOperationException("Compilação falhou:\n - " + string.Join("\n - ", errors));

        // 2. Análise de seletores — sinaliza instáveis (PRD §11).
        var warnings = actions
            .Where(a => a.Selector is not null && !SelectorGenerator.IsStableCss(a.Selector))
            .Select(a => $"Seletor instável: {a.Selector}")
            .ToList();

        // 3/4/5. Geração Robot + Browser. A estrutura (seções, indentação) é
        // garantida por Generate — ponytail: sem validação sintática redundante.
        var code = Generate(testCase);

        return new CompilationResult(code, warnings);
    }

    private static IEnumerable<string> Validate(ScriptAction a) => a.Action switch
    {
        ActionKind.Navigate when string.IsNullOrWhiteSpace(a.Value) => ["Navigate exige uma URL."],
        ActionKind.Click when string.IsNullOrWhiteSpace(a.Selector) => ["Click exige um seletor."],
        ActionKind.Input when string.IsNullOrWhiteSpace(a.Selector) || a.Value is null => ["Input exige seletor e valor."],
        ActionKind.Wait when string.IsNullOrWhiteSpace(a.Value) => ["Wait exige um valor (segundos ou seletor)."],
        ActionKind.Assert when string.IsNullOrWhiteSpace(a.Selector) => ["Assert exige um seletor."],
        _ => []
    };

    private static string Generate(TestCase testCase)
    {
        var sb = new StringBuilder();
        sb.AppendLine("*** Settings ***");
        sb.AppendLine("Library    Browser");
        sb.AppendLine();
        sb.AppendLine("*** Test Cases ***");
        sb.AppendLine(testCase.Name);

        var firstNavigationDone = false;
        foreach (var a in testCase.ManualScript)
        {
            sb.Append("    ").AppendLine(Step(a, ref firstNavigationDone));
        }
        return sb.ToString();
    }

    private static string Step(ScriptAction a, ref bool firstNavigationDone)
    {
        var value = ToRobotValue(a.Value);
        switch (a.Action)
        {
            case ActionKind.Navigate:
                var nav = firstNavigationDone ? $"Go To    {value}" : $"New Page    {value}";
                firstNavigationDone = true;
                return nav;
            case ActionKind.Click:
                return $"Click    {a.Selector}";
            case ActionKind.Input:
                return $"Fill Text    {a.Selector}    {value}";
            case ActionKind.Wait:
                return IsSeconds(a.Value!)
                    ? $"Sleep    {a.Value}s"
                    : $"Wait For Elements State    {a.Value}    visible";
            case ActionKind.Assert:
                return a.Value is null
                    ? $"Wait For Elements State    {a.Selector}    visible"
                    : $"Get Text    {a.Selector}    ==    {value}";
            default:
                throw new InvalidOperationException($"Ação não suportada: {a.Action}");
        }
    }

    private static string? ToRobotValue(string? value) =>
        value is null ? null : TemplateVar.Replace(value, "$${${1}}");

    private static bool IsSeconds(string value) => double.TryParse(value, out _);
}
