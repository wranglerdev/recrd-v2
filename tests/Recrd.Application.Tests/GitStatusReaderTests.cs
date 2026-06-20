using Recrd.Application.Git;
using Recrd.Application.Setup;

namespace Recrd.Application.Tests;

public class GitStatusReaderTests
{
    [Fact]
    public void Reads_branch_and_changed_files()
    {
        var runner = Substitute.For<IProcessRunner>();
        runner.Run("git", Arg.Is<string>(a => a.Contains("rev-parse")))
            .Returns(new ProcessResult(0, "main\n"));
        runner.Run("git", Arg.Is<string>(a => a.Contains("status")))
            .Returns(new ProcessResult(0, " M src/A.cs\n?? novo.txt\n"));

        var status = new GitStatusReader(runner).Read("/repo");

        status.IsRepository.Should().BeTrue();
        status.Branch.Should().Be("main");
        status.ChangedFiles.Should().BeEquivalentTo("src/A.cs", "novo.txt");
    }

    [Fact]
    public void Reports_not_a_repository_when_git_fails()
    {
        var runner = Substitute.For<IProcessRunner>();
        runner.Run("git", Arg.Any<string>()).Returns(new ProcessResult(128, ""));

        var status = new GitStatusReader(runner).Read("/not/a/repo");

        status.IsRepository.Should().BeFalse();
        status.ChangedFiles.Should().BeEmpty();
    }

    [Fact]
    public void Handles_short_status_lines()
    {
        var runner = Substitute.For<IProcessRunner>();
        runner.Run("git", Arg.Is<string>(a => a.Contains("rev-parse")))
            .Returns(new ProcessResult(0, "dev"));
        runner.Run("git", Arg.Is<string>(a => a.Contains("status")))
            .Returns(new ProcessResult(0, "ab\n"));

        var status = new GitStatusReader(runner).Read("/repo");

        status.ChangedFiles.Should().ContainSingle().Which.Should().Be("ab");
    }
}
