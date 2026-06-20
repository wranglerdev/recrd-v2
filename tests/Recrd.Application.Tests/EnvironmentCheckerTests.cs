using Recrd.Application.Setup;

namespace Recrd.Application.Tests;

public class EnvironmentCheckerTests
{
    private static IProcessRunner RunnerWhere(Func<string, bool> ok)
    {
        var runner = Substitute.For<IProcessRunner>();
        runner.Run(Arg.Any<string>(), Arg.Any<string>())
            .Returns(ci => new ProcessResult(ok(ci.ArgAt<string>(0)) ? 0 : 1, ""));
        return runner;
    }

    [Fact]
    public void Reports_ready_when_everything_present()
    {
        var venv = Path.Combine(Path.GetTempPath(), $"venv-{Guid.NewGuid():N}");
        Directory.CreateDirectory(venv);
        try
        {
            var checker = new EnvironmentChecker(RunnerWhere(_ => true), venv);

            var status = checker.Check();

            status.IsReady.Should().BeTrue();
        }
        finally { Directory.Delete(venv); }
    }

    [Fact]
    public void Not_ready_when_venv_missing_and_tools_fail()
    {
        var checker = new EnvironmentChecker(RunnerWhere(_ => false), "/no/such/venv");

        var status = checker.Check();

        status.PythonInstalled.Should().BeFalse();
        status.VenvExists.Should().BeFalse();
        status.IsReady.Should().BeFalse();
    }

    [Fact]
    public void Install_runs_all_steps_when_successful()
    {
        var runner = RunnerWhere(_ => true);

        new EnvironmentChecker(runner, "/tmp/venv").Install("requirements.txt").Should().BeTrue();

        runner.Received().Run("python", Arg.Is<string>(a => a.Contains("venv")));
        runner.Received().Run("pip", Arg.Is<string>(a => a.Contains("requirements.txt")));
        runner.Received().Run("rfbrowser", "init");
    }

    [Fact]
    public void Install_stops_on_first_failure()
    {
        // pip falha -> rfbrowser init não roda.
        var runner = RunnerWhere(file => file != "pip");

        new EnvironmentChecker(runner, "/tmp/venv").Install("requirements.txt").Should().BeFalse();

        runner.DidNotReceive().Run("rfbrowser", "init");
    }
}
