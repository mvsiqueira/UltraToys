using System.Windows;
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
}
