using Recrd.Application.Abstractions;
using Recrd.Application.Auditing;
using Recrd.Application.Runner;
using Recrd.Application.Setup;
using Recrd.Domain.Entities;

namespace Recrd.Application.Tests;

public class TestRunnerTests
{
    private static (TestRunner runner, IAuditTrail audit) Build(int exitCode, string log)
    {
        var proc = Substitute.For<IProcessRunner>();
        proc.Run("robot", Arg.Any<string>()).Returns(new ProcessResult(exitCode, log));

        var user = Substitute.For<IUserContext>();
        user.Username.Returns("DOMAIN\\jose.silva");

        var audit = Substitute.For<IAuditTrail>();
        return (new TestRunner(proc, user, audit), audit);
    }

    private static readonly TestCase Case = new() { Name = "Login", TestSuiteId = Guid.NewGuid() };

    [Fact]
    public void Records_passed_execution_with_user_and_log()
    {
        var (runner, audit) = Build(0, "Assertion successful");

        var exec = runner.Run(Case, "/tests/login.robot", "/out");

        exec.Result.Should().Be(ExecutionResult.Passed);
        exec.User.Should().Be("DOMAIN\\jose.silva");
        exec.Log.Should().Be("Assertion successful");
        exec.TestCaseId.Should().Be(Case.Id);
        exec.Duration.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        audit.Received().Executed(Case.Id, "Passed");
    }

    [Fact]
    public void Records_failed_execution_on_nonzero_exit()
    {
        var (runner, _) = Build(1, "FAIL");

        runner.Run(Case, "/tests/login.robot", "/out").Result.Should().Be(ExecutionResult.Failed);
    }

    [Fact]
    public void Uses_injected_clock_for_executed_at()
    {
        var proc = Substitute.For<IProcessRunner>();
        proc.Run("robot", Arg.Any<string>()).Returns(new ProcessResult(0, ""));
        var user = Substitute.For<IUserContext>();
        var fixed_ = new DateTimeOffset(2026, 6, 20, 10, 35, 0, TimeSpan.Zero);

        var exec = new TestRunner(proc, user, Substitute.For<IAuditTrail>(), new FixedClock(fixed_))
            .Run(Case, "f.robot", "/out");

        exec.ExecutedAt.Should().Be(fixed_);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
