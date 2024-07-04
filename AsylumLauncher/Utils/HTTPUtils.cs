using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AsylumLauncher.Utils
{
    public class HTTPUtils
    {
        // Retrieve data object from API
        public async Task<T> RetrieveData<T>(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        return JsonConvert.DeserializeObject<T>(responseBody)!;
                    }
                    else
                    {
                        throw new Exception($"Failed to retrieve data from API. Status code: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                throw;
            }

        }

        // Download file from URL
        public async Task<bool> DownloadFile(string url, string destinationDirectory)
        {

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string fileName = response.Content.Headers.ContentDisposition.FileName;
                            // Fuck ContentDisposition, remove all but the filename
                            fileName = Regex.Replace(fileName, @"[^\w\d._]", "");
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

    }
}
