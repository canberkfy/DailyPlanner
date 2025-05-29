using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace DailyPlanner.Services
{
    public class GptService
    {
        // ✅ Bu alanlar class'ın içinde ve constructor dışında tanımlı olmalı:
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        // ✅ Constructor
        public GptService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = configuration["OpenAI:ApiKey"];
        }

        // ✅ Ana metot
        public async Task<string> GetOptimizedScheduleAsync(string prompt)
        {
            var requestBody = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
                    new { role = "system", content = "Sen bir planlayıcısın. Verilen görevleri kullanıcıya mantıklı bir sırayla gün içine yerleştir. Sadece şu formatta cevap ver: '**08 - 10**: Ders çalış'" },
                    new { role = "user", content = prompt }
                }
            };

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://localhost:5001");
            httpClient.DefaultRequestHeaders.Add("X-Title", "DailyPlanner");

            var response = await httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var jsonString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(jsonString);

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var message = error.GetProperty("message").GetString();
                return $"⚠️ GPT API Hatası: {message}";
            }

            try
            {
                return doc.RootElement
                          .GetProperty("choices")[0]
                          .GetProperty("message")
                          .GetProperty("content")
                          .GetString();
            }
            catch
            {
                return "❌ Yanıt beklenilen formatta değil. API'den dönen içerik: \n\n" + jsonString;
            }
        }
    }
}
