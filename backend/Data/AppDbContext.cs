using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EmotionRecord> EmotionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EmotionRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Text).HasMaxLength(5000).IsRequired();
                entity.Property(e => e.Label).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Score).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasIndex(e => e.Username);
                entity.HasIndex(e => e.Label);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
