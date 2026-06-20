using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recrd.Domain.Entities;
using Recrd.Infrastructure.Data;

namespace Recrd.App;

/// <summary>Tela inicial (PRD §8, §19): últimas execuções + ações rápidas.</summary>
public partial class MainWindow : Window
{
    private readonly IServiceScopeFactory _scopes;

    public MainWindow(IServiceScopeFactory scopes)
    {
        _scopes = scopes;
        InitializeComponent();
        LoadRecentExecutions();
    }

    private void LoadRecentExecutions()
    {
        using var scope = _scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RecrdDbContext>();

        // Join com TestCase só p/ exibir o nome; 10 mais recentes (PRD §8).
        var rows = db.Executions
            .OrderByDescending(e => e.ExecutedAt)
            .Take(10)
            .Join(db.TestCases, e => e.TestCaseId, c => c.Id, (e, c) => new { e, c.Name })
            .AsNoTracking()
            .ToList()
            .Select(x => new ExecutionRow(
                x.e.Result == ExecutionResult.Passed ? "✔" : "✖",
                x.Name,
                x.e.ExecutedAt.LocalDateTime.ToString("g"),
                $"{x.e.Duration:mm\\:ss}"))
            .ToList();

        ExecutionsList.ItemsSource = rows;
        EmptyHint.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // ponytail: navegação/CRUD ainda não existem como telas — avisa em vez de fingir.
    // Substituir cada placeholder quando a tela correspondente chegar.
    private void NovoProjeto_Click(object sender, RoutedEventArgs e) => NotImplemented("Novo Projeto");
    private void ImportarMassa_Click(object sender, RoutedEventArgs e) => NotImplemented("Importar Massa");
    private void AbrirUltimoProjeto_Click(object sender, RoutedEventArgs e) => NotImplemented("Abrir Último Projeto");

    private void GravarTeste_Click(object sender, RoutedEventArgs e) => new AutomationWindow { Owner = this }.Show();

    private void Sobre_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private static void NotImplemented(string what) =>
        MessageBox.Show($"{what}: ainda não implementado.", "recrd", MessageBoxButton.OK, MessageBoxImage.Information);

    private sealed record ExecutionRow(string Icon, string Name, string When, string Duration);
}
