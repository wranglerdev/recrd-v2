using Recrd.Application.TestData;

namespace Recrd.Application.Tests;

public class MassaCsvTests
{
    [Fact]
    public void Parses_first_row_as_variables_and_second_as_values()
    {
        // PRD §7
        var massa = MassaCsv.Parse("login", "usuario,senha,email\nadmin,123456,admin@email.com");

        massa.Name.Should().Be("login");
        massa.Variables.Should().HaveCount(3);
        massa.Variables["usuario"].Should().Be("admin");
        massa.Variables["senha"].Should().Be("123456");
        massa.Variables["email"].Should().Be("admin@email.com");
    }

    [Fact]
    public void Handles_quoted_values_with_commas()
    {
        var massa = MassaCsv.Parse("m", "nome,obs\nJoao,\"a, b, c\"");

        massa.Variables["obs"].Should().Be("a, b, c");
    }

    [Fact]
    public void Rejects_when_value_row_is_missing()
    {
        var act = () => MassaCsv.Parse("m", "usuario,senha");

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Rejects_column_count_mismatch()
    {
        var act = () => MassaCsv.Parse("m", "a,b,c\n1,2");

        act.Should().Throw<FormatException>();
    }
}
