using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;
using static backend.NewFolder.EmotionDtos;

namespace backend.Services
{
    public class EmotionService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public EmotionService(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public async Task<string> AnalyzeAsync(string text, CancellationToken ct = default)
        {
            // 1) POST: event_id al
            var postUrl = "https://mustafaep-emotion-analyzer.hf.space/gradio_api/call/analyze";
            var payload = new EmotionPostPayload { data = new[] { text } };
            var json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var postResp = await _http.PostAsync(postUrl, content, ct);
            postResp.EnsureSuccessStatusCode();

            var postBody = await postResp.Content.ReadAsStringAsync(ct);
            var evt = JsonConvert.DeserializeObject<EventIdResponse>(postBody);
            if (string.IsNullOrWhiteSpace(evt?.event_id))
                throw new("event_id boş döndü.");

            // İsteğe bağlı: event_id’yi kısa süreli cache’le (log/izleme için)
            _cache.Set($"emotion:event:{evt.event_id}", text, TimeSpan.FromMinutes(5));

            // 2) Hemen ardından GET: /analyze/{event_id}
            var getUrl = $"https://mustafaep-emotion-analyzer.hf.space/gradio_api/call/analyze/{evt.event_id}";

            // Bazı servisler sonucu hazırlayana kadar 0.5–1 sn bekletir. Küçük bir retry:
            const int maxTry = 3;
            for (int attempt = 1; attempt <= maxTry; attempt++)
            {
                using var getResp = await _http.GetAsync(getUrl, ct);

                if (getResp.IsSuccessStatusCode)
                {
                    var getBody = await getResp.Content.ReadAsStringAsync(ct);
                    return getBody; // nihai analiz sonucu/yanıt
                }

                // 202/404 gibi geçici durumlarda kısa bekleyip tekrar dene
                if (attempt < maxTry)
                    await Task.Delay(TimeSpan.FromMilliseconds(700 * attempt), ct);
            }

            throw new($"Analiz sonucu alınamadı. event_id: {evt.event_id}");
        }

        
    }
}
