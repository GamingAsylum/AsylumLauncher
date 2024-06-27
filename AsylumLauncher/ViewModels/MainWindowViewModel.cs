using AsylumLauncher.Models;
using AsylumLauncher.Utils;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AsylumLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string ApiBaseUrl = "https://api.gaming-asylum.com";
        private string _latestMissionFile;

        private string _buttonContent = "Play";
        public string ButtonContent
        {
            get => _buttonContent;
            set => this.RaiseAndSetIfChanged(ref _buttonContent, value);
        }

        public ObservableCollection<News> NewsItems { get; set; }

        public MainWindowViewModel()
        {
            InitializeAsync();

            // Placeholder news
            NewsItems =
            [
                new News
                {
                    Author = "Mitch",
                    ReleaseDate = new DateOnly(2024, 06, 28),
                    Title = "Coming Soon!",
                    Description = "Server news will be coming soon in a later update!",
                },
            ];
        }

        private async void InitializeAsync()
        {
            VersionCheck serverVersions = await GetVersionsAsync();
            if (serverVersions.CurrentMissionfileVersion != "")
            {
                _latestMissionFile = serverVersions.CurrentMissionfileVersion;
            }
        }

        public async Task Launch()
        {
            bool hasLatestMission = CheckLocalVersion(_latestMissionFile);
            if (!hasLatestMission)
            {
                ButtonContent = "Downloading...";
                bool success = await DownloadLatest();

                if (success)
                {
                    ButtonContent = "Launching...";
                    await Task.Delay(1000);
                    LaunchSteamGame();
                    await Task.Delay(2000);
                    ButtonContent = "Play";
                }
            }

            ButtonContent = "Launching...";
            LaunchSteamGame();
            await Task.Delay(2000);
            ButtonContent = "Play";
        }

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
                ButtonContent = "Error";
                throw;
            }
        }

        private static async Task<bool> DownloadLatest()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(ApiBaseUrl + "/missionfile/latest"))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            if (response.Content.Headers.ContentDisposition != null && response.Content.Headers.ContentDisposition.FileName != null)
                            {
                                string fileName = response.Content.Headers.ContentDisposition.FileName;
                                // Fuck ContentDisposition, remove all but the filename
                                fileName = Regex.Replace(fileName, @"[^\w\d._]", "");
                                string destinationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arma 3", "MPMissionsCache");
                                string destinationPath = Path.Combine(destinationDirectory, fileName);

                                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                                {
                                    using (Stream fileStream = File.Create(destinationPath))
                                    {
                                        await contentStream.CopyToAsync(fileStream);
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("no file name in content-disposition");
                            }

                        }
                        else
                        {
                            throw new Exception("Failed to download file. Status code: " + response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        private static void LaunchSteamGame()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.FileName = @"steam://rungameid/107410//-noLauncher -useBE -connect=life.gaming-asylum.com";
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

        }

        private async Task<VersionCheck> GetVersionsAsync()
        {
            try
            {
                HTTPUtils api = new HTTPUtils();
                return await api.RetrieveData<VersionCheck>(ApiBaseUrl + "/missionfile/versioncheck");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                ButtonContent = "Error";
                throw;
            }
        }
    }
}
