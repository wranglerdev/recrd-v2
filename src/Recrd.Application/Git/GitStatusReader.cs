using Recrd.Application.Setup;

namespace Recrd.Application.Git;

/// <summary>Estado Git de um repositório de projeto (PRD §14).</summary>
public sealed record GitStatus(bool IsRepository, string Branch, IReadOnlyList<string> ChangedFiles);

/// <summary>
/// Lê branch atual e arquivos alterados via git (PRD §14). Não substitui um
/// cliente Git — só mostra o essencial; o diff é aberto externamente.
/// </summary>
public sealed class GitStatusReader(IProcessRunner runner)
{
    public GitStatus Read(string repoPath)
    {
        var branch = runner.Run("git", $"-C \"{repoPath}\" rev-parse --abbrev-ref HEAD");
        if (!branch.Ok)
            return new GitStatus(IsRepository: false, Branch: "", ChangedFiles: []);

        // Formato porcelain: "XY caminho" (os 2 primeiros chars são status + 1 espaço).
        var status = runner.Run("git", $"-C \"{repoPath}\" status --porcelain");
        var changed = status.StdOut
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .Select(line => line.Length > 3 ? line[3..] : line)
            .ToList();

        return new GitStatus(IsRepository: true, Branch: branch.StdOut.Trim(), ChangedFiles: changed);
    }
}
