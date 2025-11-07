using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmotionController : ControllerBase
    {
        private readonly EmotionService _service;
        public EmotionController(EmotionService service) => _service = service;

        public class AnalyzeRequest { public string Text { get; set; } }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest req, CancellationToken ct)
        {
            var resultJson = await _service.AnalyzeAsync(req.Text, ct);
            return Content(resultJson, "application/json");
        }
    }

    public class EmotionRequest
    {
        public string Text { get; set; }
    }
}
