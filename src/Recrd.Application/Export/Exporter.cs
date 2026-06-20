using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Recrd.Application.Compilation;
using Recrd.Domain.Entities;

namespace Recrd.Application.Export;

public sealed record ExportArtifact(string FileName, string Content);

/// <summary>
/// Gera os artefatos exportáveis (PRD §17): script bruto JSON, script Robot
/// compilado e log de execução. O conteúdo é puro; <see cref="WriteAll"/> grava.
/// </summary>
public static class Exporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static ExportArtifact RawScript(TestCase tc) =>
        new($"{Slug(tc.Name)}.recrd.json", JsonSerializer.Serialize(tc.ManualScript, JsonOptions));

    public static ExportArtifact CompiledScript(TestCase tc)
    {
        var robot = tc.CompiledScript ?? RobotCompiler.Compile(tc).RobotCode;
        return new($"{Slug(tc.Name)}.robot", robot);
    }

    public static ExportArtifact ExecutionLog(Execution exec) =>
        new($"execution-{exec.ExecutedAt:yyyy-MM-dd}.log", exec.Log ?? string.Empty);

    /// <summary>Grava script bruto + compilado em <paramref name="targetDir"/>.</summary>
    public static IReadOnlyList<string> WriteAll(TestCase tc, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        var paths = new List<string>();
        foreach (var artifact in new[] { RawScript(tc), CompiledScript(tc) })
        {
            var path = Path.Combine(targetDir, artifact.FileName);
            File.WriteAllText(path, artifact.Content);
            paths.Add(path);
        }
        return paths;
    }

    // ponytail: slug ASCII simples (minúsculas, não-alfanumérico -> hífen). Basta p/
    // nomes de teste; trocar por transliteração se aparecer acento/unicode relevante.
    private static string Slug(string name)
    {
        var sb = new StringBuilder(name.Length);
        foreach (var c in name.Trim().ToLowerInvariant())
            sb.Append(char.IsLetterOrDigit(c) ? c : '-');
        return string.Join('-', sb.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
