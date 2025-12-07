using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using static backend.Dtos.EmotionDtos;

namespace backend.Services
{
    public class EmotionService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmotionService> _logger;
        private readonly string _baseUrl;
        private readonly int _maxRetries;
        private readonly int _retryDelayMs;

        public EmotionService(
            HttpClient http,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<EmotionService> logger)
        {
            _http = http;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
            
            _baseUrl = _configuration["EmotionService:BaseUrl"]
                ?? Environment.GetEnvironmentVariable("EMOTION_SERVICE_BASE_URL")
                ?? "https://mustafaep-emotion-analyzer.hf.space";
            
            _maxRetries = int.Parse(_configuration["EmotionService:MaxRetries"] ?? "3");
            _retryDelayMs = int.Parse(_configuration["EmotionService:RetryDelayMs"] ?? "700");
        }

        public async Task<string> AnalyzeAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be empty.", nameof(text));
            }

            var postUrl = $"{_baseUrl}/gradio_api/call/analyze";
            var payload = new EmotionPostPayload { data = new[] { text } };
            var json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                using var postResp = await _http.PostAsync(postUrl, content, ct);
                postResp.EnsureSuccessStatusCode();

                var postBody = await postResp.Content.ReadAsStringAsync(ct);
                var evt = JsonConvert.DeserializeObject<EventIdResponse>(postBody);
                
                if (string.IsNullOrWhiteSpace(evt?.event_id))
                {
                    _logger.LogError("event_id returned empty. Response: {Response}", postBody);
                    throw new InvalidOperationException("event_id returned empty.");
                }

                _cache.Set($"emotion:event:{evt.event_id}", text, TimeSpan.FromMinutes(5));

                var getUrl = $"{_baseUrl}/gradio_api/call/analyze/{evt.event_id}";

                // Exponential backoff retry mechanism
                for (int attempt = 1; attempt <= _maxRetries; attempt++)
                {
                    try
                    {
                        using var getResp = await _http.GetAsync(getUrl, ct);

                        if (getResp.IsSuccessStatusCode)
                        {
                            var getBody = await getResp.Content.ReadAsStringAsync(ct);
                            _logger.LogInformation("Analysis successful. event_id: {EventId}, attempt: {Attempt}", evt.event_id, attempt);
                            return getBody;
                        }

                        _logger.LogWarning("Could not retrieve analysis result. Status: {Status}, attempt: {Attempt}/{MaxRetries}", 
                            getResp.StatusCode, attempt, _maxRetries);

                        if (attempt < _maxRetries)
                        {
                            var delay = TimeSpan.FromMilliseconds(_retryDelayMs * Math.Pow(2, attempt - 1));
                            await Task.Delay(delay, ct);
                        }
                    }
                    catch (Exception ex) when (attempt < _maxRetries)
                    {
                        _logger.LogWarning(ex, "Error retrieving analysis result. attempt: {Attempt}/{MaxRetries}", attempt, _maxRetries);
                        var delay = TimeSpan.FromMilliseconds(_retryDelayMs * Math.Pow(2, attempt - 1));
                        await Task.Delay(delay, ct);
                    }
                }

                throw new TimeoutException($"Could not retrieve analysis result. event_id: {evt.event_id}, max retries: {_maxRetries}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed. URL: {Url}", postUrl);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error.");
                throw;
            }
        }
    }
}
