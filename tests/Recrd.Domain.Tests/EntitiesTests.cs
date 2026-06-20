using Recrd.Domain.Entities;

namespace Recrd.Domain.Tests;

public class EntitiesTests
{
    [Fact]
    public void Builds_the_full_project_hierarchy()
    {
        var project = new Project { Name = "Banco XYZ", RobotPath = "/repo" };
        var plan = new TestPlan { Name = "Smoke", ProjectId = project.Id };
        var suite = new TestSuite { Name = "Login", TestPlanId = plan.Id };
        var testCase = new TestCase { Name = "Login OK", TestSuiteId = suite.Id };

        project.Plans.Add(plan);
        plan.Suites.Add(suite);
        suite.Cases.Add(testCase);

        project.Plans.Single().Suites.Single().Cases.Single().Should().BeSameAs(testCase);
        testCase.Status.Should().Be(TestCaseStatus.Draft);
    }

    [Fact]
    public void Script_action_keeps_mass_variable_as_template()
    {
        // PRD §12: dragging a mass variable stores {{usuario}}, not the resolved value.
        var action = new ScriptAction(ActionKind.Input, "#email", "{{usuario}}");

        action.Value.Should().Be("{{usuario}}");
    }
}
