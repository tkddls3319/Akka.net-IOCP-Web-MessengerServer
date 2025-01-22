using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session
{
    public class WebManager
    {
        static readonly WebManager _instance = new WebManager();
        public static WebManager Instance => _instance;

        readonly HttpClient _httpClient;
        string _baseUrl => "https://localhost:7022";

        public WebManager()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_baseUrl),
                DefaultRequestHeaders =
                {
                    Accept = {new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")},
                },
                Timeout = TimeSpan.FromSeconds(15),
            };
        }

        #region Post
        public void SendPostRequest<T>(string url, object req, Action<T> res)
        {
            Task.Run(async () =>
            {
                try
                {
                    string postData = System.Text.Json.JsonSerializer.Serialize(req);
                    HttpResponseMessage response = await _httpClient.PostAsync($"/api/{url}", new StringContent(postData, Encoding.UTF8, "application/json"));

                    response.EnsureSuccessStatusCode(); 

                    string data = await response.Content.ReadAsStringAsync();
                    T result = System.Text.Json.JsonSerializer.Deserialize<T>(data);

                    res.Invoke(result);

                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }
        public async Task<T> SendPostRequest<T>(string url, object req)
        {
            try
            {
                string postData = System.Text.Json.JsonSerializer.Serialize(req);

                HttpResponseMessage response = await _httpClient.PostAsync($"/api/{url}", new StringContent(postData, Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();

                string data = await response.Content.ReadAsStringAsync();
                T result = System.Text.Json.JsonSerializer.Deserialize<T>(data);

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("API 요청 실패: " + ex.Message);
            }
        }
        #endregion
    }
}
