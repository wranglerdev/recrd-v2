using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recrd.Infrastructure.Data;

namespace Recrd.App;

/// <summary>
/// Tela de automação (PRD §9): header + sidebar + Browser Sandbox.
/// </summary>
public partial class AutomationWindow : Window
{
    public AutomationWindow(IServiceScopeFactory scopes)
    {
        InitializeComponent();
        Sandbox.ActionCaptured += OnActionCaptured;
        Sandbox.ElementInspected += OnElementInspected;
        InspectToggle.Checked += (_, _) => Sandbox.InspectMode = true;
        InspectToggle.Unchecked += (_, _) => Sandbox.InspectMode = false;
        LoadMassaVariables(scopes);
    }

    private void LoadMassaVariables(IServiceScopeFactory scopes)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RecrdDbContext>();
        // Nomes de variáveis de todas as massas — arrastáveis p/ campos (PRD §12).
        MassasList.ItemsSource = db.Massas.AsNoTracking()
            .ToList()
            .SelectMany(m => m.Variables.Keys)
            .Distinct()
            .OrderBy(k => k)
            .ToList();
    }

    // Inicia o drag da variável selecionada (PRD §12).
    private void MassasList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && MassasList.SelectedItem is string variable)
            DragDrop.DoDragDrop(MassasList, variable, DragDropEffects.Copy);
    }

    // Gravação: cada ação capturada entra na Timeline; seletor instável avisa (PRD §11).
    private void OnActionCaptured(object? sender, CapturedAction e)
    {
        var a = e.Action;
        var label = a.Action switch
        {
            Recrd.Domain.Entities.ActionKind.Navigate => $"Navigate → {a.Value}",
            Recrd.Domain.Entities.ActionKind.Input => $"Input {a.Selector} = {a.Value}",
            _ => $"{a.Action} {a.Selector}",
        };
        if (e.Selector is { IsStable: false })
            label = "⚠ " + label;
        Timeline.Items.Add(label);
    }

    // Modo Inspect (PRD §10): mostra elemento/ID/classes/XPath sob o mouse.
    private void OnElementInspected(object? sender, InspectedElement e)
    {
        Inspector.Text =
            $"Elemento: <{e.Element.Tag}>\n" +
            $"ID: {e.Element.Id ?? "—"}\n" +
            $"Classes: {e.Classes ?? "—"}\n" +
            $"Seletor: {e.Selector?.Value ?? "—"}";
    }

    private void Play_Click(object sender, RoutedEventArgs e) => TODO("Play");
    private void Pause_Click(object sender, RoutedEventArgs e) => TODO("Pause");
    private void Stop_Click(object sender, RoutedEventArgs e) => TODO("Stop");
    private void Reload_Click(object sender, RoutedEventArgs e) => Sandbox.Reload();
    private void Exportar_Click(object sender, RoutedEventArgs e) => TODO("Exportar");
    private void Compilar_Click(object sender, RoutedEventArgs e) => TODO("Compilar");

    private static void TODO(string what) =>
        MessageBox.Show($"{what}: ainda não implementado.", "recrd", MessageBoxButton.OK, MessageBoxImage.Information);
}
