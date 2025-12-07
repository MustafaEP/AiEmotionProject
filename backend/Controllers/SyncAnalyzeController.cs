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
            [Required(ErrorMessage = "Username is required.")]
            [MinLength(1, ErrorMessage = "Username must be at least 1 character.")]
            [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Text is required.")]
            [MinLength(1, ErrorMessage = "Text must be at least 1 character.")]
            [MaxLength(5000, ErrorMessage = "Text cannot exceed 5000 characters.")]
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
                return BadRequest("username and text are required.");

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
                    _logger.LogWarning("EmotionController response failed. Status: {Status}, Username: {Username}", 
                        resp.StatusCode, req.Username);
                    return StatusCode((int)resp.StatusCode, new
                    {
                        error = "EmotionController response failed.",
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
                    _logger.LogError(ex, "Failed to parse response. Username: {Username}", req.Username);
                    return BadRequest(new
                    {
                        error = "Failed to parse response.",
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
                
                _logger.LogInformation("Synchronous analysis saved. Username: {Username}, Label: {Label}, Score: {Score}", 
                    record.Username, record.Label, record.Score);
                
                return Ok(new
                {
                    message = "Synchronous analysis saved.",
                    username = record.Username,
                    text = record.Text,
                    label = record.Label,
                    score = record.Score,
                    createdAt = record.CreatedAt
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed. Username: {Username}", req.Username);
                return StatusCode(503, new { error = "Could not connect to external service." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error. Username: {Username}", req.Username);
                return StatusCode(500, new { error = "An error occurred." });
            }
        }
    }
}
