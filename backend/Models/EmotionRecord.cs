using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class EmotionRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Text { get; set; } 

        [Required]
        public double Score { get; set; }  // 0.0 - 1.0 

        [Required]
        public string Label { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
