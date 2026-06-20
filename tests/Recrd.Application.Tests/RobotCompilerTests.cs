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
    public void Second_navigation_uses_go_to()
    {
        var result = RobotCompiler.Compile(CaseWith(
            new ScriptAction(ActionKind.Navigate, Value: "https://a"),
            new ScriptAction(ActionKind.Navigate, Value: "https://b")));

        result.RobotCode.Should().Contain("New Page    https://a");
        result.RobotCode.Should().Contain("Go To    https://b");
    }

    [Fact]
    public void Numeric_wait_becomes_sleep()
    {
        var result = RobotCompiler.Compile(CaseWith(new ScriptAction(ActionKind.Wait, Value: "2")));

        result.RobotCode.Should().Contain("Sleep    2s");
    }

    [Fact]
    public void Selector_wait_becomes_wait_for_elements_state()
    {
        var result = RobotCompiler.Compile(CaseWith(new ScriptAction(ActionKind.Wait, Value: "#spinner")));

        result.RobotCode.Should().Contain("Wait For Elements State    #spinner    visible");
    }

    [Fact]
    public void Assert_without_value_waits_for_visible()
    {
        var result = RobotCompiler.Compile(CaseWith(new ScriptAction(ActionKind.Assert, "#msg")));

        result.RobotCode.Should().Contain("Wait For Elements State    #msg    visible");
    }

    [Theory]
    [InlineData(ActionKind.Navigate, null, null)]   // sem URL
    [InlineData(ActionKind.Input, null, "v")]        // sem seletor
    [InlineData(ActionKind.Input, "#x", null)]       // sem valor
    [InlineData(ActionKind.Wait, null, null)]        // sem valor
    [InlineData(ActionKind.Assert, null, null)]      // sem seletor
    public void Rejects_invalid_actions(ActionKind kind, string? selector, string? value)
    {
        var act = () => RobotCompiler.Compile(CaseWith(new ScriptAction(kind, selector, value)));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rejects_unsupported_action_kind()
    {
        var act = () => RobotCompiler.Compile(CaseWith(new ScriptAction((ActionKind)999, "#x", "v")));

        act.Should().Throw<InvalidOperationException>();
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
