using Recrd.Domain.Selectors;

namespace Recrd.Domain.Tests;

public class SelectorGeneratorTests
{
    [Fact]
    public void Prefers_data_testid_above_everything()
    {
        var el = new ElementInfo("input", TestId: "login", Id: "x", Name: "y");

        var sel = SelectorGenerator.Generate(el);

        sel.Strategy.Should().Be(SelectorStrategy.TestId);
        sel.Value.Should().Be("[data-testid=\"login\"]");
        sel.IsStable.Should().BeTrue();
    }

    [Fact]
    public void Falls_through_priority_to_id_when_higher_options_missing()
    {
        var el = new ElementInfo("input", Id: "email");

        var sel = SelectorGenerator.Generate(el);

        sel.Strategy.Should().Be(SelectorStrategy.Id);
        sel.Value.Should().Be("#email");
    }

    [Fact]
    public void Flags_positional_css_as_unstable()
    {
        // PRD §11: div:nth-child(5) -> baixa confiança -> alerta.
        var el = new ElementInfo("div", Css: "div:nth-child(5)");

        var sel = SelectorGenerator.Generate(el);

        sel.Strategy.Should().Be(SelectorStrategy.Css);
        sel.IsStable.Should().BeFalse();
    }

    [Fact]
    public void Xpath_fallback_is_always_low_confidence()
    {
        var el = new ElementInfo("div", XPath: "//div[@data-x]");

        var sel = SelectorGenerator.Generate(el);

        sel.Strategy.Should().Be(SelectorStrategy.XPath);
        sel.IsStable.Should().BeFalse();
    }

    [Fact]
    public void Rejects_absolute_xpath()
    {
        // PRD §11: nunca gerar XPath absoluto.
        var act = () => SelectorGenerator.Generate(new ElementInfo("div", XPath: "/html/body/div[5]"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Throws_when_no_selectable_attribute()
    {
        var act = () => SelectorGenerator.Generate(new ElementInfo("div"));

        act.Should().Throw<InvalidOperationException>();
    }
}
