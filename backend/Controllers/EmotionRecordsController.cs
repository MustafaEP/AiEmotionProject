using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmotionRecordsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public EmotionRecordsController(AppDbContext db) => _db = db;

        // --- READ: Listeleme (filtre + sayfalama) ---
        // GET: /api/emotionrecords?username=erhan&label=negative&page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? username,
            [FromQuery] string? label,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var q = _db.EmotionRecords.AsQueryable();

            if (!string.IsNullOrWhiteSpace(username))
                q = q.Where(x => x.Username == username);

            if (!string.IsNullOrWhiteSpace(label))
                q = q.Where(x => x.Label == label);

            if (fromUtc.HasValue)
                q = q.Where(x => x.CreatedAt >= fromUtc.Value);

            if (toUtc.HasValue)
                q = q.Where(x => x.CreatedAt <= toUtc.Value);

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

        // --- READ: Tek kayıt ---
        // GET: /api/emotionrecords/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var rec = await _db.EmotionRecords.FindAsync(id);
            if (rec is null) return NotFound(new { error = "Kayıt bulunamadı." });
            return Ok(rec);
        }

        public class UpdateEmotionRecordDto
        {
            public string? Username { get; set; }
            public string? Text { get; set; }
            public string? Label { get; set; }
            public double? Score { get; set; } // 0-1 arası
        }

        // --- DELETE: Tek kayıt sil ---
        // DELETE: /api/emotionrecords/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var rec = await _db.EmotionRecords.FindAsync(id);
            if (rec is null) return NotFound(new { error = "Kayıt bulunamadı." });

            _db.EmotionRecords.Remove(rec);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
