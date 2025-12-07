using backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmotionController : ControllerBase
    {
        private readonly EmotionService _service;
        private readonly ILogger<EmotionController> _logger;

        public EmotionController(EmotionService service, ILogger<EmotionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public class AnalyzeRequest
        {
            [Required(ErrorMessage = "Text field is required.")]
            [MinLength(1, ErrorMessage = "Text must be at least 1 character.")]
            [MaxLength(5000, ErrorMessage = "Text cannot exceed 5000 characters.")]
            public string Text { get; set; } = string.Empty;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest req, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(req.Text))
                {
                    return BadRequest(new { error = "Text field cannot be empty." });
                }

                var resultJson = await _service.AnalyzeAsync(req.Text, ct);
                return Content(resultJson, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during analysis.");
                return StatusCode(500, new { error = "An error occurred during analysis." });
            }
        }
    }
}
