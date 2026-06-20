namespace Recrd.Application.Setup;

/// <summary>Resultado de um processo externo.</summary>
public readonly record struct ProcessResult(int ExitCode, string StdOut)
{
    public bool Ok => ExitCode == 0;
}

/// <summary>
/// Abstração para rodar processos externos (python, robot, playwright).
/// Permite testar a lógica de ambiente sem depender do SO (PRD §29).
/// </summary>
public interface IProcessRunner
{
    ProcessResult Run(string fileName, string arguments);
}
