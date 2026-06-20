using Microsoft.EntityFrameworkCore;
using Recrd.Domain.Entities;
using Recrd.Infrastructure.Data;

namespace Recrd.Infrastructure.Tests.Data;

// PRD §23: base SQLite temporária por teste de integração.
public sealed class RecrdDbContextTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"recrd-test-{Guid.NewGuid():N}.db");

    private RecrdDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<RecrdDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;
        var ctx = new RecrdDbContext(options);
        ctx.Database.Migrate(); // exercita as migrations, não só o modelo
        return ctx;
    }

    [Fact]
    public void All_dbsets_are_queryable()
    {
        using var ctx = NewContext();

        ctx.Projects.Count().Should().Be(0);
        ctx.TestPlans.Count().Should().Be(0);
        ctx.TestSuites.Count().Should().Be(0);
        ctx.TestCases.Count().Should().Be(0);
        ctx.Executions.Count().Should().Be(0);
        ctx.Massas.Count().Should().Be(0);
    }

    [Fact]
    public void Persists_project_tree_and_round_trips_manual_script()
    {
        var caseId = Guid.NewGuid();
        using (var ctx = NewContext())
        {
            var project = new Project { Name = "Banco XYZ", RobotPath = "/repo" };
            var plan = new TestPlan { Name = "Smoke", ProjectId = project.Id };
            var suite = new TestSuite { Name = "Login", TestPlanId = plan.Id };
            var testCase = new TestCase { Id = caseId, Name = "Login OK", TestSuiteId = suite.Id };
            testCase.ManualScript.Add(new ScriptAction(ActionKind.Input, "#email", "{{usuario}}"));

            project.Plans.Add(plan);
            plan.Suites.Add(suite);
            suite.Cases.Add(testCase);

            ctx.Projects.Add(project);
            ctx.SaveChanges();
        }

        using (var ctx = NewContext())
        {
            var reloaded = ctx.TestCases.Single(c => c.Id == caseId);
            reloaded.ManualScript.Should().ContainSingle()
                .Which.Should().Be(new ScriptAction(ActionKind.Input, "#email", "{{usuario}}"));
        }
    }

    public void Dispose()
    {
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
