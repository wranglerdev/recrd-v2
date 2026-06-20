using System.Text.Json;
using System.Text.Json.Serialization;

namespace Recrd.Application.Diagnostics;

/// <summary>
/// Metadados de reprodutibilidade do build, exibidos na tela Sobre (PRD §30).
/// Geração no build via version.json (target MSBuild em Recrd.App).
/// </summary>
public sealed record BuildInfo(
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("gitCommit")] string GitCommit,
    [property: JsonPropertyName("buildDate")] string BuildDate,
    [property: JsonPropertyName("target")] string Target)
{
    public static BuildInfo FromJson(string json) =>
        JsonSerializer.Deserialize<BuildInfo>(json)
        ?? throw new FormatException("version.json inválido.");
}
