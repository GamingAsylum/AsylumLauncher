using AsylumLauncher.Models;
using AsylumLauncher.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Diagnostics;
using System;
using AsylumLauncher.Utils;
using Avalonia.Input;

namespace AsylumLauncher.Views
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            this.FindControl<Border>("Popup")!.IsVisible = false;

            // Subscribe to the OpenPopupRequested event
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OpenPopupRequested += (message, type) =>
                {
                    ShowPopup(message, type);
                };
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                await viewModel.Launch();
            }
        }

        private async void ListBoxItem_MouseDoubleClick(object sender, TappedEventArgs e)
        {
            // Handle double-click event here
            if (sender is Grid grid && grid.DataContext is News item)
            {
                try
                {
                    if (item.NewsURL != null)
                        Process.Start(new ProcessStartInfo(item.NewsURL) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Logger.Log("Error opening URL: " + ex.Message);
                }
            }
        }

        private void ClosePopup_Click(object sender, RoutedEventArgs e)
        {
            this.FindControl<Border>("Popup")!.IsVisible = false;
        }

        public void ShowPopup(string message, PopupType type)
        {
            var popup = this.FindControl<Border>("Popup")!;
            popup.IsVisible = true;

            Color color = new();

            switch (type)
            {
                case PopupType.Error:
                    color = Color.Parse("#80de2e21");
                    break;
                case PopupType.Info:
                    color = Color.Parse("#80041A88");
                    break;
            }

            popup.Background = new SolidColorBrush(color);

            var viewModel = DataContext as MainWindowViewModel;
            if (viewModel != null)
            {
                viewModel.PopupMessage = message;
            }
        }

        public enum PopupType
        {
            Error,
            Info
        }

    }
}