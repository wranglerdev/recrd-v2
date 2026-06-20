using Recrd.Infrastructure.Diagnostics;

namespace Recrd.Infrastructure.Tests.Diagnostics;

public sealed class BuildInfoLoaderTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"recrd-bi-{Guid.NewGuid():N}");

    [Fact]
    public void Loads_build_info_from_version_json_next_to_app()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "version.json"),
            """{ "version": "1.2.3", "gitCommit": "abc1234", "buildDate": "2026-06-20T14:35:00Z", "target": "win-x64" }""");

        var info = BuildInfoLoader.LoadFrom(_dir);

        info.Version.Should().Be("1.2.3");
        info.Target.Should().Be("win-x64");
    }

    [Fact]
    public void Returns_unknown_when_version_json_missing()
    {
        var info = BuildInfoLoader.LoadFrom(_dir);

        info.Version.Should().Be("desconhecida");
        info.GitCommit.Should().Be("unknown");
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }
}
