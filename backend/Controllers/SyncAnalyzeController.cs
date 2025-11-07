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
    private readonly IHttpClientFactory _http;
    private readonly AppDbContext _db;

    public SyncAnalyzeController(IHttpClientFactory http, AppDbContext db)
    {
        _http = http;
        _db = db;
    }

    public class SyncAnalyzeRequest { public string Username { get; set; } public string Text { get; set; } }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SyncAnalyzeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req?.Username) || string.IsNullOrWhiteSpace(req?.Text))
            return BadRequest("username ve text zorunludur.");

        var client = _http.CreateClient("SelfApi");

        // ENV okunmadıysa isteğin geldiği hostu kullan
        if (client.BaseAddress == null || string.IsNullOrWhiteSpace(client.BaseAddress.ToString()))
        {
            var inferred = $"{Request.Scheme}://{Request.Host.Value}/";
            client.BaseAddress = new Uri(inferred);
        }

        var payload = new { username = req.Username, text = req.Text };
        var json = JsonConvert.SerializeObject(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage resp;
        string body = null;
        try
        {
            // EmotionController'a iç POST
            resp = await client.PostAsync("api/emotion/analyze", content);
            body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                return StatusCode((int)resp.StatusCode, new {
                    error = "EmotionController yanıtı başarısız",
                    status = resp.StatusCode.ToString(),
                    baseAddress = client.BaseAddress?.ToString(),
                    preview = body?.Length > 200 ? body[..200] : body
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(502, new {
                error = "Self API çağrısı başarısız",
                detail = ex.Message,
                baseAddress = client.BaseAddress?.ToString()
            });
        }

        // SSE/JSON ayrıştır
        string label; double score;
        try
        {
            (label, score) = EmotionParser.ParseLabelScore(body);
        }
        catch (Exception ex)
        {
            return BadRequest(new {
                error = "Yanıt ayrıştırılamadı",
                detail = ex.Message,
                preview = body?.Length > 300 ? body[..300] : body
            });
        }

        // DB'ye yaz
        var rec = new EmotionRecord {
            Username = req.Username,
            Text = req.Text,
            Label = label,
            Score = score,
            CreatedAt = DateTime.UtcNow
        };
        _db.EmotionRecords.Add(rec);
        await _db.SaveChangesAsync();

        return Ok(new {
            message = "Senkron analiz kaydedildi.",
            username = rec.Username,
            text = rec.Text,
            label = rec.Label,
            score = rec.Score,
            createdAt = rec.CreatedAt
        });
    }
}
}
