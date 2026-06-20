namespace Recrd.Infrastructure.Storage;

/// <summary>
/// Layout local-first em %LOCALAPPDATA%/recrd/ (PRD §4). Sem servidor — tudo
/// pertence ao usuário logado. Em Linux (dev) resolve para ~/.local/share/recrd.
/// O <paramref name="root"/> é injetável para testes.
/// </summary>
public sealed class RecrdPaths
{
    public RecrdPaths(string? root = null)
    {
        Root = root ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "recrd");
    }

    public string Root { get; }
    public string DatabaseFile => Path.Combine(Root, "database.sqlite");
    public string LogsDir => Path.Combine(Root, "logs");
    public string ExecutionsLogsDir => Path.Combine(LogsDir, "executions");
    public string ExportsDir => Path.Combine(Root, "exports");
    public string CacheDir => Path.Combine(Root, "cache");
    public string SettingsFile => Path.Combine(Root, "settings.json");

    /// <summary>Cria toda a árvore de diretórios (idempotente).</summary>
    public void EnsureCreated()
    {
        Directory.CreateDirectory(ExecutionsLogsDir); // cria Root e logs/ no caminho
        Directory.CreateDirectory(ExportsDir);
        Directory.CreateDirectory(CacheDir);
    }
}
