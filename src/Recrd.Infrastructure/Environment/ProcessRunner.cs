using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Recrd.Application.Setup;

namespace Recrd.Infrastructure.Setup;

/// <summary>Executa processos externos de verdade (PRD §14).</summary>
[ExcludeFromCodeCoverage(Justification = "Glue de System.Diagnostics.Process; lógica está em EnvironmentChecker.")]
public sealed class ProcessRunner : IProcessRunner
{
    public ProcessResult Run(string fileName, string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(fileName, arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            })!;
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return new ProcessResult(process.ExitCode, stdout);
        }
        catch (Exception)
        {
            // Executável ausente (ex.: python não instalado) => trata como falha.
            return new ProcessResult(-1, string.Empty);
        }
    }
}
