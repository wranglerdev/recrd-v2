using Recrd.Application.Export;
using Recrd.Domain.Entities;

namespace Recrd.Application.Tests;

public class ExporterTests
{
    private static TestCase SampleCase()
    {
        var tc = new TestCase { Name = "Login OK", TestSuiteId = Guid.NewGuid() };
        tc.ManualScript.Add(new ScriptAction(ActionKind.Navigate, Value: "https://x"));
        tc.ManualScript.Add(new ScriptAction(ActionKind.Click, "#login"));
        return tc;
    }

    [Fact]
    public void Raw_script_is_json_named_after_the_case()
    {
        var artifact = Exporter.RawScript(SampleCase());

        artifact.FileName.Should().Be("login-ok.recrd.json");
        artifact.Content.Should().Contain("Navigate").And.Contain("#login");
    }

    [Fact]
    public void Compiled_script_is_robot_named_after_the_case()
    {
        var artifact = Exporter.CompiledScript(SampleCase());

        artifact.FileName.Should().Be("login-ok.robot");
        artifact.Content.Should().Contain("*** Test Cases ***");
    }

    [Fact]
    public void Execution_log_is_named_by_date()
    {
        var exec = new Execution
        {
            TestCaseId = Guid.NewGuid(),
            ExecutedAt = new DateTimeOffset(2026, 6, 20, 10, 35, 0, TimeSpan.Zero),
            Log = "10:35:01 Click login button",
        };

        var artifact = Exporter.ExecutionLog(exec);

        artifact.FileName.Should().Be("execution-2026-06-20.log");
        artifact.Content.Should().Be("10:35:01 Click login button");
    }

    [Fact]
    public void WriteAll_writes_raw_and_compiled_to_disk()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"recrd-export-{Guid.NewGuid():N}");
        try
        {
            var written = Exporter.WriteAll(SampleCase(), dir);

            written.Should().HaveCount(2);
            File.Exists(Path.Combine(dir, "login-ok.recrd.json")).Should().BeTrue();
            File.Exists(Path.Combine(dir, "login-ok.robot")).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }
    }
}
