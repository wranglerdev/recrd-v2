using Recrd.Application.Compilation;
using Recrd.Domain.Entities;

namespace Recrd.Application.Tests;

public class RobotCompilerTests
{
    private static TestCase CaseWith(params ScriptAction[] actions)
    {
        var tc = new TestCase { Name = "Login OK", TestSuiteId = Guid.NewGuid() };
        tc.ManualScript.AddRange(actions);
        return tc;
    }

    [Fact]
    public void Generates_runnable_robot_skeleton_with_browser_library()
    {
        var result = RobotCompiler.Compile(CaseWith(
            new ScriptAction(ActionKind.Navigate, Value: "https://x"),
            new ScriptAction(ActionKind.Click, "[data-testid=\"login\"]"),
            new ScriptAction(ActionKind.Input, "#email", "admin"),
            new ScriptAction(ActionKind.Assert, "#msg", "Welcome")));

        result.RobotCode.Should().Contain("*** Settings ***");
        result.RobotCode.Should().Contain("Library    Browser");
        result.RobotCode.Should().Contain("*** Test Cases ***");
        result.RobotCode.Should().Contain("Login OK");
        result.RobotCode.Should().Contain("New Page    https://x");
        result.RobotCode.Should().Contain("Click    [data-testid=\"login\"]");
        result.RobotCode.Should().Contain("Fill Text    #email    admin");
        result.RobotCode.Should().Contain("Get Text    #msg    ==    Welcome");
    }

    [Fact]
    public void Translates_mass_template_into_robot_variable()
    {
        // PRD §12: {{usuario}} no script -> ${usuario} no Robot.
        var result = RobotCompiler.Compile(CaseWith(
            new ScriptAction(ActionKind.Input, "#email", "{{usuario}}")));

        result.RobotCode.Should().Contain("Fill Text    #email    ${usuario}");
    }

    [Fact]
    public void Warns_on_unstable_selector()
    {
        var result = RobotCompiler.Compile(CaseWith(
            new ScriptAction(ActionKind.Click, "div:nth-child(5)")));

        result.Warnings.Should().ContainSingle().Which.Should().Contain("nth-child(5)");
    }

    [Fact]
    public void Rejects_click_without_selector()
    {
        var act = () => RobotCompiler.Compile(CaseWith(new ScriptAction(ActionKind.Click)));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rejects_empty_script()
    {
        var act = () => RobotCompiler.Compile(CaseWith());

        act.Should().Throw<InvalidOperationException>();
    }
}
