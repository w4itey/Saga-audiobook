using Audiobookshelf.Mobile.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Audiobookshelf.Mobile.Services
{
    public class AudiobookshelfApiClient
    {
        private readonly HttpClient _httpClient;

        public AudiobookshelfApiClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LoginResponse> LoginAsync(string serverUrl, string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{serverUrl}/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<LoginResponse>(responseContent);
            }

            return null;
        }

        public async Task<List<Library>> GetLibrariesAsync(string serverUrl, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{serverUrl}/api/libraries");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var librariesResponse = JsonSerializer.Deserialize<LibrariesResponse>(responseContent);
                return librariesResponse?.Libraries;
            }

            return null;
        }
    }
}
