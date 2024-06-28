using Newtonsoft.Json;
using System;
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
                        throw new Exception($"Failed to retrieve data from API. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    throw;
                }
            }
        }

        // Get filename from http response
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

    }
}
