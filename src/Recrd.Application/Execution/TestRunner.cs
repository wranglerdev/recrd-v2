using System.Diagnostics;
using Recrd.Application.Abstractions;
using Recrd.Application.Setup;
using Recrd.Domain.Entities;

namespace Recrd.Application.Runner;

/// <summary>
/// Executa um caso de teste via Robot Framework e registra a <see cref="Execution"/>
/// com usuário, data, duração, resultado e log (PRD §15).
/// </summary>
public sealed class TestRunner(IProcessRunner runner, IUserContext user, TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;

    public Execution Run(TestCase testCase, string robotFilePath, string outputDir)
    {
        var startedAt = _clock.GetUtcNow();
        var timestamp = Stopwatch.GetTimestamp();

        var result = runner.Run("robot", $"--outputdir \"{outputDir}\" \"{robotFilePath}\"");

        return new Execution
        {
            TestCaseId = testCase.Id,
            ExecutedAt = startedAt,
            Duration = Stopwatch.GetElapsedTime(timestamp),
            Result = result.Ok ? ExecutionResult.Passed : ExecutionResult.Failed,
            User = user.Username,
            Log = result.StdOut,
        };
    }
}
