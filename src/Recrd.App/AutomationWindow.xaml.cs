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
    }

    // ponytail: handlers da barra ficam como esqueleto até o sandbox emitir um
    // script e o compilador ter o que consumir. Cada um vira ação real então.
    private void Play_Click(object sender, RoutedEventArgs e) => TODO("Play");
    private void Pause_Click(object sender, RoutedEventArgs e) => TODO("Pause");
    private void Stop_Click(object sender, RoutedEventArgs e) => TODO("Stop");
    private void Reload_Click(object sender, RoutedEventArgs e) => TODO("Reload");
    private void Exportar_Click(object sender, RoutedEventArgs e) => TODO("Exportar");
    private void Compilar_Click(object sender, RoutedEventArgs e) => TODO("Compilar");

    private static void TODO(string what) =>
        MessageBox.Show($"{what}: ainda não implementado.", "recrd", MessageBoxButton.OK, MessageBoxImage.Information);
}
