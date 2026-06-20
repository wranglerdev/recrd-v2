using Recrd.Application.Abstractions;
using Recrd.Infrastructure.Auth;

namespace Recrd.Infrastructure.Tests.Auth;

public class MockUserContextTests
{
    [Fact]
    public void Exposes_the_fixed_Linux_dev_identity()
    {
        IUserContext ctx = new MockUserContext();

        ctx.Username.Should().Be("dev");
        ctx.DisplayName.Should().Be("Linux Developer");
        ctx.Domain.Should().Be("LOCAL");
    }
}
