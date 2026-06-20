using Recrd.Application.Diagnostics;

namespace Recrd.Application.Tests;

public class BuildInfoTests
{
    [Fact]
    public void Parses_version_json()
    {
        var json = """
            {
              "version": "1.0.0",
              "gitCommit": "a4f8d22",
              "buildDate": "2026-06-20T14:35:00Z",
              "target": "win-x64"
            }
            """;

        var info = BuildInfo.FromJson(json);

        info.Version.Should().Be("1.0.0");
        info.GitCommit.Should().Be("a4f8d22");
        info.BuildDate.Should().Be("2026-06-20T14:35:00Z");
        info.Target.Should().Be("win-x64");
    }

    [Fact]
    public void Rejects_invalid_json()
    {
        var act = () => BuildInfo.FromJson("null");

        act.Should().Throw<FormatException>();
    }
}
