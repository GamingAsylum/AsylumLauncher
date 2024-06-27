using AsylumLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AsylumLauncher.Utils
{
    public class HTTPUtils
    {

        public async Task<T> RetrieveData<T>(string url)
        {

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        return JsonConvert.DeserializeObject<T>(responseBody)!;

                    }
                    else
                    {
                        Console.WriteLine($"Failed to retrieve data from API. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            return default;
        }

        // get filename from http response

        private string GetFileNameFromResponse(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentDisposition != null && response.Content.Headers.ContentDisposition.FileName != null)
            {
                return Regex.Replace(response.Content.Headers.ContentDisposition.FileName, @"[^\w\d._]", "");
            }
            else
            {
                throw new Exception("no file name in content-disposition");
            }
        }

        public async Task<bool> DownloadFile()
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

    }
}
