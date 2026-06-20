namespace Recrd.Application.Setup;

/// <summary>Estado do ambiente de automação (PRD §14).</summary>
public sealed record EnvironmentStatus(
    bool PythonInstalled,
    bool VenvExists,
    bool RobotInstalled,
    bool BrowserInstalled)
{
    public bool IsReady => PythonInstalled && VenvExists && RobotInstalled && BrowserInstalled;
}

/// <summary>
/// Verifica Python, venv, Robot Framework e browser Playwright; oferece o
/// "Instalar ambiente" de um clique (PRD §14).
/// </summary>
public sealed class EnvironmentChecker(IProcessRunner runner, string venvPath)
{
    public EnvironmentStatus Check() => new(
        PythonInstalled: runner.Run("python", "--version").Ok,
        VenvExists: Directory.Exists(venvPath),
        RobotInstalled: runner.Run("robot", "--version").Ok,
        BrowserInstalled: runner.Run("rfbrowser", "show-trace --help").Ok);

    /// <summary>
    /// Instala o ambiente: cria venv, instala requirements e o browser do
    /// Playwright (rfbrowser init). Para na primeira falha (fail fast).
    /// </summary>
    public bool Install(string requirementsPath)
    {
        var steps = new[]
        {
            ("python", $"-m venv \"{venvPath}\""),
            ("pip", $"install -r \"{requirementsPath}\""),
            ("rfbrowser", "init"),
        };
        foreach (var (file, args) in steps)
            if (!runner.Run(file, args).Ok)
                return false;
        return true;
    }
}
