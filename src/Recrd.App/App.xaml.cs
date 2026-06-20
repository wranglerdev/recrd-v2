using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recrd.Infrastructure.DependencyInjection;

namespace Recrd.App;

/// <summary>
/// Composition root (PRD §31): monta configuração + DI antes de abrir a janela.
/// </summary>
public partial class App : System.Windows.Application
{
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        Services = new ServiceCollection()
            .AddRecrdInfrastructure(configuration)
            .AddSingleton<MainWindow>()
            .BuildServiceProvider();

        Services.GetRequiredService<MainWindow>().Show();
    }
}
