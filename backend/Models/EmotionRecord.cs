using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class EmotionRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(5000)]
        public string Text { get; set; } = string.Empty;

        [Required]
        [Range(0.0, 1.0, ErrorMessage = "Score 0.0 ile 1.0 arasında olmalıdır.")]
        public double Score { get; set; }

        [Required]
        [MaxLength(50)]
        public string Label { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
