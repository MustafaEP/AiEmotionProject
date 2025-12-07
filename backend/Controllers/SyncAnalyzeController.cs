using backend.Data;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncAnalyzeController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly AppDbContext _db;
        private readonly ILogger<SyncAnalyzeController> _logger;

        public SyncAnalyzeController(
            IHttpClientFactory httpFactory,
            AppDbContext db,
            ILogger<SyncAnalyzeController> logger)
        {
            _httpFactory = httpFactory;
            _db = db;
            _logger = logger;
        }

        public class SyncAnalyzeRequest
        {
            [Required(ErrorMessage = "Username zorunludur.")]
            [MinLength(1, ErrorMessage = "Username en az 1 karakter olmalıdır.")]
            [MaxLength(100, ErrorMessage = "Username en fazla 100 karakter olabilir.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Text zorunludur.")]
            [MinLength(1, ErrorMessage = "Text en az 1 karakter olmalıdır.")]
            [MaxLength(5000, ErrorMessage = "Text en fazla 5000 karakter olabilir.")]
            public string Text { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SyncAnalyzeRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(req?.Username) || string.IsNullOrWhiteSpace(req?.Text))
                return BadRequest("username ve text zorunludur.");

            var client = _httpFactory.CreateClient("SelfApi");

            var payload = new { username = req.Username, text = req.Text };
            var json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resp = await client.PostAsync("api/emotion/analyze", content);
            var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("EmotionController yanıtı başarısız. Status: {Status}, Username: {Username}", 
                        resp.StatusCode, req.Username);
                    return StatusCode((int)resp.StatusCode, new
                    {
                        error = "EmotionController yanıtı başarısız.",
                        status = resp.StatusCode.ToString(),
                        preview = body?.Length > 200 ? body[..200] : body
                    });
                }

                string label;
                double score;
                try
                {
                    (label, score) = EmotionParser.ParseLabelScore(body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Yanıt ayrıştırılamadı. Username: {Username}", req.Username);
                    return BadRequest(new
                    {
                        error = "Yanıt ayrıştırılamadı.",
                        detail = ex.Message,
                        preview = body?.Length > 200 ? body[..200] : body
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
                
                _logger.LogInformation("Senkron analiz kaydedildi. Username: {Username}, Label: {Label}, Score: {Score}", 
                    record.Username, record.Label, record.Score);
                
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP isteği başarısız. Username: {Username}", req.Username);
                return StatusCode(503, new { error = "Dış servise bağlanılamadı." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata. Username: {Username}", req.Username);
                return StatusCode(500, new { error = "Bir hata oluştu." });
            }
        }
    }
}
