namespace Recrd.Infrastructure.Robot;

/// <summary>
/// Cria/valida a estrutura padrão de um repositório Robot Framework (PRD §14).
/// </summary>
public static class RobotRepositoryScaffold
{
    private const string SampleTest =
        """
        *** Settings ***
        Library    Browser

        *** Test Cases ***
        Login
            New Page    https://example.com
        """;

    private const string Requirements =
        """
        robotframework
        robotframework-browser
        """;

    private const string GitIgnore =
        """
        reports/
        .venv/
        __pycache__/
        """;

    public static void CreateNew(string path)
    {
        foreach (var dir in new[] { "tests", "resources", "variables", "data", "reports" })
            Directory.CreateDirectory(Path.Combine(path, dir));

        File.WriteAllText(Path.Combine(path, "tests", "login.robot"), SampleTest);
        File.WriteAllText(Path.Combine(path, "requirements.txt"), Requirements);
        File.WriteAllText(Path.Combine(path, ".gitignore"), GitIgnore);
    }

    /// <summary>Um repositório existente é utilizável se já tem a pasta <c>tests/</c>.</summary>
    public static bool IsValidExisting(string path) =>
        Directory.Exists(Path.Combine(path, "tests"));
}
