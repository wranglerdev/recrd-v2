namespace Recrd.Domain.Tests;

// ponytail: baseline smoke test — proves the test toolchain (xUnit + FluentAssertions
// + NSubstitute) is wired. Real domain/application tests replace this as code lands.
public class ToolchainSmokeTests
{
    [Fact]
    public void FluentAssertions_is_available()
    {
        (2 + 2).Should().Be(4);
    }

    [Fact]
    public void NSubstitute_is_available()
    {
        var calc = Substitute.For<IAdder>();
        calc.Add(2, 3).Returns(5);
        calc.Add(2, 3).Should().Be(5);
    }

    public interface IAdder
    {
        int Add(int a, int b);
    }
}
