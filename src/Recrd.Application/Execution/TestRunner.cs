using System.Diagnostics;
using Recrd.Application.Abstractions;
using Recrd.Application.Auditing;
using Recrd.Application.Setup;
using Recrd.Domain.Entities;

namespace Recrd.Application.Runner;

/// <summary>
/// Executa um caso de teste via Robot Framework e registra a <see cref="Execution"/>
/// com usuário, data, duração, resultado e log (PRD §15). Emite evento de auditoria (§16).
/// </summary>
public sealed class TestRunner(IProcessRunner runner, IUserContext user, IAuditTrail audit, TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    public Execution Run(TestCase testCase, string robotFilePath, string outputDir)
    {
        var startedAt = _clock.GetUtcNow();
        var timestamp = Stopwatch.GetTimestamp();

        var result = runner.Run("robot", $"--outputdir \"{outputDir}\" \"{robotFilePath}\"");

        var execution = new Execution
        {
            TestCaseId = testCase.Id,
            ExecutedAt = startedAt,
            Duration = Stopwatch.GetElapsedTime(timestamp),
            Result = result.Ok ? ExecutionResult.Passed : ExecutionResult.Failed,
            User = user.Username,
            Log = result.StdOut,
        };

        audit.Executed(execution.TestCaseId, execution.Result.ToString());
        return execution;
    }
}
