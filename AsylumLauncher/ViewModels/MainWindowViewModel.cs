using AsylumLauncher.Models;
using AsylumLauncher.Utils;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Controls;
using Avalonia.Input;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace AsylumLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string ApiBaseUrl = "https://api.gaming-asylum.com";
        private const float LauncherVersion = 0.1f;

        private string LatestMissionFile;


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
                    ReleaseDate = new DateTime(2024, 04, 08, 12, 00, 00),
                    Title = "Coming Soon!",
                    Description = "Server news will be coming soon in a later update",
                },
            ];
        }

        private async void InitializeAsync()
        {
            VersionCheck serverVersions = await GetVersionsAsync();
            LatestMissionFile = serverVersions.CurrentMissionfileVersion;
        }

        public async Task Launch()
        {
            bool success = false;
            if (!await CheckLocalVersion(LatestMissionFile))
            {
                success = await DownloadLatest();
            }

            if (success) {
                LaunchSteamGame();
            }
        }

        private async Task<Boolean> CheckLocalVersion(string missionFileToCheck)
        {
            string missionFileDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Arma 3", "MPMissionsCache");
            string filePath = Path.Combine(missionFileDir, $"{missionFileToCheck}.pbo");

            return File.Exists(filePath);
        }

        private async Task<bool> DownloadLatest()
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
                            } else
                            {
                                throw new Exception("no file name in content-disposition");
                            }

                        }
                        else
                        {
                            Console.WriteLine("Failed to download file. Status code: " + response.StatusCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error downloading the mission file" + ex.Message);
            }
            return false;
        }

        private static void LaunchSteamGame()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = @"steam://rungameid/107410//-noLauncher -useBE -connect=life.gaming-asylum.com";
            Process.Start(startInfo);
        }

        private async Task<VersionCheck> GetVersionsAsync()
        {
            VersionCheck version = new VersionCheck();
            try
            {
                HTTPUtils api = new HTTPUtils();
                version = await api.RetrieveData<VersionCheck>(ApiBaseUrl + "/missionfile/versioncheck");

            } catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return version;
        }
    }
}
