using Recrd.Infrastructure.Storage;

namespace Recrd.Infrastructure.Tests.Storage;

public class RecrdPathsTests
{
    [Fact]
    public void Lays_out_the_expected_local_first_structure()
    {
        var root = Path.Combine(Path.GetTempPath(), $"recrd-paths-{Guid.NewGuid():N}");
        var paths = new RecrdPaths(root);

        // PRD §4
        paths.DatabaseFile.Should().Be(Path.Combine(root, "database.sqlite"));
        paths.LogsDir.Should().Be(Path.Combine(root, "logs"));
        paths.ExecutionsLogsDir.Should().Be(Path.Combine(root, "logs", "executions"));
        paths.ExportsDir.Should().Be(Path.Combine(root, "exports"));
        paths.CacheDir.Should().Be(Path.Combine(root, "cache"));
        paths.SettingsFile.Should().Be(Path.Combine(root, "settings.json"));
    }

    [Fact]
    public void EnsureCreated_creates_all_directories()
    {
        var root = Path.Combine(Path.GetTempPath(), $"recrd-paths-{Guid.NewGuid():N}");
        var paths = new RecrdPaths(root);
        try
        {
            paths.EnsureCreated();

            Directory.Exists(paths.ExecutionsLogsDir).Should().BeTrue();
            Directory.Exists(paths.ExportsDir).Should().BeTrue();
            Directory.Exists(paths.CacheDir).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
