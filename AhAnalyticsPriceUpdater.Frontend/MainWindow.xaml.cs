using System.Windows;
using AhAnalyticsPriceUpdater.Frontend.Models;

namespace AhAnalyticsPriceUpdater.Frontend;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(UpdateProcessViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}