using backend.Data;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncAnalyzeController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly AppDbContext _db;

        public SyncAnalyzeController(IHttpClientFactory httpFactory, AppDbContext db)
        {
            _httpFactory = httpFactory;
            _db = db;
        }

        public class SyncAnalyzeRequest
        {
            public string Username { get; set; }
            public string Text { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SyncAnalyzeRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.Username) || string.IsNullOrWhiteSpace(req?.Text))
                return BadRequest("username ve text zorunludur.");

            var client = _httpFactory.CreateClient("SelfApi");

            var payload = new { username = req.Username, text = req.Text };
            var json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("api/emotion/analyze", content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new
                {
                    error = "EmotionController yanıtı başarısız.",
                    status = resp.StatusCode.ToString(),
                    preview = body?.Length > 200 ? body.Substring(0, 200) : body
                });
            }

            string label;
            double score;
            try
            {
                (label, score) = EmotionParser.ParseLabelScore(body);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new
                {
                    error = "Yanıt ayrıştırılamadı.",
                    detail = ex.Message,
                    preview = body?.Length > 200 ? body.Substring(0, 200) : body
                });
            }

            var record = new EmotionRecord
            {
                Username = req.Username,
                Text = req.Text,
                Label = label,
                Score = score,
                CreatedAt = DateTime.UtcNow
            };

            _db.EmotionRecords.Add(record);
            await _db.SaveChangesAsync();
            return Ok(new
            {
                message = "Senkron analiz kaydedildi.",
                username = record.Username,
                text = record.Text,
                label = record.Label,
                score = record.Score,
                createdAt = record.CreatedAt
            });
        }
    }
}
