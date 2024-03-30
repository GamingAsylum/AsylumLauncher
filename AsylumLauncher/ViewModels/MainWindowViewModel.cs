using AsylumLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AsylumLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string DownloadUrl = "TBD";

        public ObservableCollection<News> NewsItems { get; set; }

        public MainWindowViewModel()
        {
            NewsItems = GetTestNews();
        }

        public async Task DownloadAndLaunch()
        {

            await DownloadFile(DownloadUrl);
            LaunchSteamGame();
        }

        private async Task DownloadFile(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url))
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
            catch (WebException ex)
            {
                Console.WriteLine("oopsie woopsie");
            }
        }

        private static void LaunchSteamGame()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = @"steam://rungameid/107410//-noLauncher -useBE -connect=life.gaming-asylum.com";
            Process.Start(startInfo);
        }

        private static ObservableCollection<News> GetTestNews()
        {
            ObservableCollection<News> result = new ObservableCollection<News>();
            for (int i = 0; i < 10; i++)
            {
                News newNews = new News();
                newNews.Title = $"Title {i}";
                newNews.Description = $"description for item {i} \n more description \n more description";
                newNews.Author = "Mitch";
                newNews.ReleaseDate = DateOnly.FromDateTime(DateTime.Now).AddDays(-i);

                result.Add(newNews);
            }

            return result;
        }
    }
}
