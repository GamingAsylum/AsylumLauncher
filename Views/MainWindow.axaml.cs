using AsylumLauncher.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace AsylumLauncher.Views
{
    public partial class MainWindow : Window
    {

        private double originalWidth;
        private double originalHeight;

        public MainWindow()
        {
            InitializeComponent();
            this.originalWidth = this.Width;
            this.originalHeight = this.Height;
            this.Resized += OnResized;
        }

        private void OnResized(object sender, EventArgs e)
        {
            // Calculate new dimensions while maintaining the original aspect ratio
            double newWidth = this.Width;
            double newHeight = this.originalHeight * (newWidth / this.originalWidth);

            // Adjust UI elements
            this.Width = newWidth;
            this.Height = newHeight;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.DownloadAndLaunch();
            }
        }
    }
}