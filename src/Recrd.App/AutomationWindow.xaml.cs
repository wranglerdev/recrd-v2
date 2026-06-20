using System.Windows;

namespace Recrd.App;

/// <summary>
/// Tela de automação (PRD §9): casca de layout (header + sidebar + sandbox).
/// Os controles do header ganham comportamento conforme o sandbox/compilador
/// são plugados nas próximas issues.
/// </summary>
public partial class AutomationWindow : Window
{
    public AutomationWindow()
    {
        InitializeComponent();
        Sandbox.ActionCaptured += OnActionCaptured;
        Sandbox.ElementInspected += OnElementInspected;
        InspectToggle.Checked += (_, _) => Sandbox.InspectMode = true;
        InspectToggle.Unchecked += (_, _) => Sandbox.InspectMode = false;
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
