using AsylumLauncher.Models;
using AsylumLauncher.Utils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static AsylumLauncher.Views.MainWindow;

namespace AsylumLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string ApiBaseUrl = "https://api.gaming-asylum.com";
        private const float CurrentLauncherVersion = 0.2f;

        private bool _missionFileUpToDate = false;

        #region Properties
        private string _popupMessage = "";
        public string PopupMessage
        {
            get => _popupMessage;
            set => this.RaiseAndSetIfChanged(ref _popupMessage, value);
        }

        private string _consoleText = "downloading mission file...";
        public string ConsoleText
        {
            get => _consoleText;
            set => this.RaiseAndSetIfChanged(ref _consoleText, value);
        }

        private string _launcherVersion = "";
        public string LauncherVersion
        {
            get => _launcherVersion;
            set => this.RaiseAndSetIfChanged(ref _launcherVersion, value);
        }

        public ObservableCollection<News> _newsItems;
        public ObservableCollection<News> NewsItems
        {
            get => _newsItems;
            set => this.RaiseAndSetIfChanged(ref _newsItems, value);
        }

        public event Action<string, PopupType> OpenPopupRequested;
        #endregion

        public MainWindowViewModel()
        {
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            LauncherVersion = $"v{CurrentLauncherVersion}";

            ConsoleText = "Checking for launcher updates...";
            await CheckForLauncherUpdatesAsync();

            ConsoleText = "Checking for news...";
            NewsItems = new ObservableCollection<News>(await GetLatestNewsAsync());

            ConsoleText = "Checking for mission file updates...";
            _missionFileUpToDate = await CheckVersionsAsync();
            if (_missionFileUpToDate)
            {
                ConsoleText = "Mission file is up to date...";
            }
            else
            {
                ConsoleText = "Mission file requires an update...";
            }
        }

        private async Task CheckForLauncherUpdatesAsync()
        {
            try
            {
                HTTPUtils api = new HTTPUtils();
                var LatestLauncherVersion = await api.RetrieveData<AsylumLauncherVersion>(ApiBaseUrl + "/asylumlauncher/versioncheck");

                if (CurrentLauncherVersion <= LatestLauncherVersion.ForceUpdateVersion)
                {
                    OpenPopup(LatestLauncherVersion.ForceUpdateMessage ?? "A new launcher version is available. Please download the latest version from the website.", PopupType.Error);
                }
                else if (CurrentLauncherVersion <= LatestLauncherVersion.WarningVersion)
                {
                    OpenPopup(LatestLauncherVersion.WarningMessage ?? "A new launcher version is available. Please download the latest version from the website.", PopupType.Info);
                }
                else if (CurrentLauncherVersion < LatestLauncherVersion.LatestVersion)
                {
                    OpenPopup("A new launcher version is available. Please download the latest version from the website.", PopupType.Info);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ConsoleText = "Failed checking for launcher updates...";
                OpenPopup("Failed checking for launcher updates.", PopupType.Error);
            }
        }

        // Launch the game when the play button is clicked
        public async Task Launch()
        {
            if (!_missionFileUpToDate)
            {
                ConsoleText = "Downloading mission file...";

                HTTPUtils api = new HTTPUtils();
                // Download the latest mission file
                bool success = await api.DownloadFile(ApiBaseUrl + "/missionfile/latest", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arma 3", "MPMissionsCache"));

                if (success)
                {
                    ConsoleText = "Launching game...";
                    LaunchSteamGame();
                }
                else
                {
                    ConsoleText = "Failed to download mission file...";
                    OpenPopup("Failed to download mission file. Please ensure that you have an active internet connection.", PopupType.Error);
                }
            }
            else
            {
                ConsoleText = "Launching game...";
                LaunchSteamGame();
            }
        }

        // Check if the local version is the latest version
        private bool CheckLocalVersion(string missionFileToCheck)
        {
            try
            {
                string missionFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arma 3", "MPMissionsCache");
                string filePath = Path.Combine(missionFileDir, $"{missionFileToCheck}.pbo");

                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                OpenPopup("An error occurred while checking local files for the latest mission file.", PopupType.Error);
                return false;
            }
        }

        // Launch Arma 3 with the correct parameters
        private async void LaunchSteamGame()
        {
            try
            {
                ConsoleText = "Launching game...";
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.FileName = @"steam://rungameid/107410//-noLauncher -useBE -connect=life.gaming-asylum.com";
                Process.Start(startInfo);
                ConsoleText = "Game launched successfully...";
                await Task.Delay(5000);
                ConsoleText = "";
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ConsoleText = "Failed to launch game...";
                OpenPopup("Failed to launch game. Please ensure that Arma 3 is installed and that the Steam client is running.", PopupType.Error);
            }

        }

        // Retrieve latest news from backend
        private async Task<List<News>> GetLatestNewsAsync()
        {
            try
            {
                HTTPUtils api = new HTTPUtils();
                return await api.RetrieveData<List<News>>(ApiBaseUrl + "/news");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ConsoleText = "Failed to retrieve news...";
                List<News> news =
                [
                    new News
                    {
                        Title = "Failed to retrieve news...",
                        Description = "The launcher has failed to retrieve news from the backend, This will not impact launcher functionality.",
                        Author = "Asylum",
                    },
                ];
                return news;
            }
        }

        // Check if local version is the latest version
        private async Task<bool> CheckVersionsAsync()
        {
            try
            {
                HTTPUtils api = new HTTPUtils();
                var currentVersion = await api.RetrieveData<MissionFileVersion>(ApiBaseUrl + "/missionfile/versioncheck");

                return CheckLocalVersion(currentVersion.CurrentMissionfileVersion);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ConsoleText = "Failed checking for mission file updates...";
                OpenPopup("Failed checking for mission file updates. Latest mission file will be downloaded", PopupType.Error);
                return false;
            }
        }

        // Method to trigger opening the popup
        public void OpenPopup(string message, PopupType type)
        {
            OpenPopupRequested?.Invoke(message, type);
        }
    }
}
