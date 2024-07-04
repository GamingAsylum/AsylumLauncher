using AsylumLauncher.Models;
using AsylumLauncher.Utils;
using AsylumLauncher.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AsylumLauncher.Views
{
    public partial class MainWindow : Window
    {

        private readonly string[] wallpapers = {
            "avares://AsylumLauncher/Assets/asylum_wallpaper2.png",
            "avares://AsylumLauncher/Assets/asylum_wallpaper3.png",
            "avares://AsylumLauncher/Assets/asylum_wallpaper4.png",
            "avares://AsylumLauncher/Assets/asylum_wallpaper5.png",
            "avares://AsylumLauncher/Assets/asylum_wallpaper6.png"
        };

        public MainWindow()
        {
            InitializeComponent();

            RandomizeWallpaper();

            this.FindControl<Border>("Popup")!.IsVisible = false;

            DataContext = new MainWindowViewModel();

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

        private void ListBoxItem_MouseDoubleClick(object sender, TappedEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is News item)
            {
                try
                {
                    if (item.NewsURL != null)
                    {
                        // Regular expression pattern for a valid HTTP or HTTPS URL
                        string pattern = @"^(http|https)://[\w-]+(\.[\w-]+)+([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?$";
                        Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                        // Check if the URL is valid before opening it
                        if (regex.IsMatch(item.NewsURL))
                        {
                            // Launch the URL in the default browser
                            Process.Start(new ProcessStartInfo(item.NewsURL) { UseShellExecute = true });
                        }
                    }
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

        private void RandomizeWallpaper()
        {
            Random random = new Random();
            string selectedWallpaper = wallpapers[random.Next(wallpapers.Length)];

            var imageControl = this.FindControl<Image>("WallpaperImage");
            imageControl.Source = new Bitmap(AssetLoader.Open(new Uri(selectedWallpaper)));
        }

        public enum PopupType
        {
            Error,
            Info
        }

    }
}