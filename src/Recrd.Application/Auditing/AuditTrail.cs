using Microsoft.Extensions.Logging;

namespace Recrd.Application.Auditing;

/// <summary>
/// Registra os eventos importantes para auditoria (PRD §16) como log estruturado.
/// Senhas nunca devem ser passadas aqui (PRD §18).
/// </summary>
public interface IAuditTrail
{
    void MassaImported(string massaName, int variableCount);
    void TestChanged(Guid testCaseId);
    void Compiled(Guid testCaseId, int warningCount);
    void Exported(Guid testCaseId, string fileName);
    void Executed(Guid testCaseId, string result);
}

public sealed class AuditTrail(ILogger<AuditTrail> logger) : IAuditTrail
{
    public void MassaImported(string massaName, int variableCount) =>
        logger.LogInformation("Massa importada: {MassaName} ({VariableCount} variáveis)", massaName, variableCount);

    public void TestChanged(Guid testCaseId) =>
        logger.LogInformation("Teste alterado: {TestCaseId}", testCaseId);

    public void Compiled(Guid testCaseId, int warningCount) =>
        logger.LogInformation("Compilação concluída: {TestCaseId} ({WarningCount} avisos)", testCaseId, warningCount);

    public void Exported(Guid testCaseId, string fileName) =>
        logger.LogInformation("Exportação: {TestCaseId} -> {FileName}", testCaseId, fileName);

    public void Executed(Guid testCaseId, string result) =>
        logger.LogInformation("Execução: {TestCaseId} = {Result}", testCaseId, result);
}
