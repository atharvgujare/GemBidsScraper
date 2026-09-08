using System.Text;
using System.Text.Json;
using GemBidScraper.DTOs.Python;

namespace GemBidScraper.Services
{
    public class PythonApiService
    {
        private readonly HttpClient _httpClient;

        public PythonApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PythonApiResponseDto?> ParsePdfAsync(string filePath)
        {
            var request = new PythonRequestDto
            {
                FilePath = filePath
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "http://127.0.0.1:8000/extract-file",
                content);

            if (!response.IsSuccessStatusCode)
                return null;

            var responseJson =
                await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PythonApiResponseDto>(
                responseJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        public async Task<JsonDocument> ParseOnlinePagesAsync(int pages)
        {
            var request = new
            {
                Pages = pages
            };

            var json = JsonSerializer.Serialize(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "http://127.0.0.1:8000/extract-online-pages",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            return JsonDocument.Parse(responseJson);
        }

    }
}