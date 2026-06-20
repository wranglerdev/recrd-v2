using System.Windows;
using Recrd.Infrastructure.Diagnostics;

namespace Recrd.App;

/// <summary>Tela Sobre: metadados de build p/ auditoria corporativa (PRD §30).</summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        var info = BuildInfoLoader.LoadFrom(AppContext.BaseDirectory);
        VersionText.Text = info.Version;
        CommitText.Text = info.GitCommit;
        BuildDateText.Text = info.BuildDate;
        TargetText.Text = info.Target;
    }
}
