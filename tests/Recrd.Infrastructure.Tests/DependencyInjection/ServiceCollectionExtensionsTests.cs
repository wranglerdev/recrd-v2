using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recrd.Application.Abstractions;
using Recrd.Infrastructure.Data;
using Recrd.Infrastructure.DependencyInjection;
using Recrd.Infrastructure.Storage;

namespace Recrd.Infrastructure.Tests.DependencyInjection;

public sealed class ServiceCollectionExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"recrd-di-{Guid.NewGuid():N}");

    private ServiceProvider BuildProvider()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Recrd:StorageRoot"] = _root })
            .Build();
        return new ServiceCollection().AddRecrdInfrastructure(config).BuildServiceProvider();
    }

    [Fact]
    public void Resolves_the_infrastructure_graph()
    {
        using var sp = BuildProvider();

        sp.GetRequiredService<IUserContext>().Should().NotBeNull();
        sp.GetRequiredService<RecrdPaths>().Root.Should().Be(_root);
        sp.GetRequiredService<RecrdDbContext>().Should().NotBeNull();
    }

    [Fact]
    public void Creates_the_storage_tree_on_registration()
    {
        using var sp = BuildProvider();

        Directory.Exists(Path.Combine(_root, "logs", "executions")).Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
