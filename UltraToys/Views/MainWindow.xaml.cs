using System.Windows;
using System.Windows.Controls;
using UltraToys.ViewModels;

namespace UltraToys.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public void AttachViewModel(MainViewModel viewModel)
    {
        DataContext = viewModel;
    }

    // Acordeão: ao expandir uma seção, recolhe as demais.
    private void OnSectionExpanded(object sender, RoutedEventArgs e)
    {
        // Durante o carregamento o estilo dispara Expanded em todas as seções; só vale a ação do usuário.
        if (!IsLoaded) return;
        if (e.OriginalSource is not Expander opened || sender is not System.Windows.Controls.Panel panel) return;

        foreach (var section in panel.Children.OfType<Expander>())
        {
            if (!ReferenceEquals(section, opened))
                section.IsExpanded = false;
        }
    }
}
