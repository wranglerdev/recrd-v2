using Recrd.Application.Diagnostics;

namespace Recrd.Infrastructure.Diagnostics;

/// <summary>
/// Carrega o version.json gerado no build (PRD §30) p/ a tela Sobre.
/// Tolera ausência do arquivo (dev/builds locais) devolvendo metadados "desconhecidos".
/// </summary>
public static class BuildInfoLoader
{
    public static BuildInfo LoadFrom(string directory)
    {
        var path = Path.Combine(directory, "version.json");
        return File.Exists(path)
            ? BuildInfo.FromJson(File.ReadAllText(path))
            : new BuildInfo("desconhecida", "unknown", "unknown", "unknown");
    }
}
