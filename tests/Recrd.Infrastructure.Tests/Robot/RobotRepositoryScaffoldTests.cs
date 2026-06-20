using Recrd.Infrastructure.Robot;

namespace Recrd.Infrastructure.Tests.Robot;

public sealed class RobotRepositoryScaffoldTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"robot-repo-{Guid.NewGuid():N}");

    [Fact]
    public void CreateNew_lays_out_the_standard_structure()
    {
        RobotRepositoryScaffold.CreateNew(_root);

        // PRD §14
        foreach (var dir in new[] { "tests", "resources", "variables", "data", "reports" })
            Directory.Exists(Path.Combine(_root, dir)).Should().BeTrue($"{dir} deve existir");

        File.Exists(Path.Combine(_root, "tests", "login.robot")).Should().BeTrue();
        File.Exists(Path.Combine(_root, "requirements.txt")).Should().BeTrue();
        File.Exists(Path.Combine(_root, ".gitignore")).Should().BeTrue();
        File.ReadAllText(Path.Combine(_root, "requirements.txt")).Should().Contain("robotframework");
    }

    [Fact]
    public void Existing_repo_is_valid_when_it_has_a_tests_folder()
    {
        Directory.CreateDirectory(Path.Combine(_root, "tests"));

        RobotRepositoryScaffold.IsValidExisting(_root).Should().BeTrue();
    }

    [Fact]
    public void Existing_repo_is_invalid_without_tests_folder()
    {
        Directory.CreateDirectory(_root);

        RobotRepositoryScaffold.IsValidExisting(_root).Should().BeFalse();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
