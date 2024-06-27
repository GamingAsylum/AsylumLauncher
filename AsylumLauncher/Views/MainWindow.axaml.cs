using AsylumLauncher.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AsylumLauncher.Views
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.Launch();
            }
        }

    }
}