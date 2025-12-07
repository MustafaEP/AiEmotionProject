using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmotionRecordsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EmotionRecordsController> _logger;

        public EmotionRecordsController(AppDbContext db, ILogger<EmotionRecordsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? username,
            [FromQuery] string? label,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Input validation
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;
            
            if (!string.IsNullOrWhiteSpace(username) && username.Length > 100)
            {
                return BadRequest(new { error = "Username en fazla 100 karakter olabilir." });
            }
            
            if (!string.IsNullOrWhiteSpace(label) && label.Length > 50)
            {
                return BadRequest(new { error = "Label en fazla 50 karakter olabilir." });
            }

            var q = _db.EmotionRecords.AsQueryable();

            if (!string.IsNullOrWhiteSpace(username))
                q = q.Where(x => x.Username == username);

            if (!string.IsNullOrWhiteSpace(label))
                q = q.Where(x => x.Label == label);

            if (fromUtc.HasValue)
                q = q.Where(x => x.CreatedAt >= fromUtc.Value);

            if (toUtc.HasValue)
                q = q.Where(x => x.CreatedAt <= toUtc.Value);

            try
            {
                var total = await q.CountAsync();

                var items = await q
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new
                {
                    page,
                    pageSize,
                    total,
                    items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıtlar getirilirken hata. Filters: Username={Username}, Label={Label}", username, label);
                return StatusCode(500, new { error = "Bir hata oluştu." });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var rec = await _db.EmotionRecords.FindAsync(id);
                if (rec is null)
                {
                    _logger.LogWarning("Kayıt bulunamadı. Id: {Id}", id);
                    return NotFound(new { error = "Kayıt bulunamadı." });
                }
                return Ok(rec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt getirilirken hata. Id: {Id}", id);
                return StatusCode(500, new { error = "Bir hata oluştu." });
            }
        }

        public class UpdateEmotionRecordDto
        {
            [MaxLength(100)]
            public string? Username { get; set; }
            
            [MaxLength(5000)]
            public string? Text { get; set; }
            
            [MaxLength(50)]
            public string? Label { get; set; }
            
            [Range(0.0, 1.0, ErrorMessage = "Score 0.0 ile 1.0 arasında olmalıdır.")]
            public double? Score { get; set; }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                var rec = await _db.EmotionRecords.FindAsync(id);
                if (rec is null)
                {
                    _logger.LogWarning("Silinecek kayıt bulunamadı. Id: {Id}", id);
                    return NotFound(new { error = "Kayıt bulunamadı." });
                }

                _db.EmotionRecords.Remove(rec);
                await _db.SaveChangesAsync();
                
                _logger.LogInformation("Kayıt silindi. Id: {Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt silinirken hata. Id: {Id}", id);
                return StatusCode(500, new { error = "Bir hata oluştu." });
            }
        }
    }
}
