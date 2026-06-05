using MediaTracker.Modules.MediaData.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaTracker.Modules.MediaData;

public class MediaDataDbContext : DbContext
{
    public MediaDataDbContext(DbContextOptions<MediaDataDbContext> options) : base(options) { }

    public DbSet<MediaDataEntry> MediaDataEntries => Set<MediaDataEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaDataEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MediaId).IsRequired();
            entity.Property(e => e.DataType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(2000).IsRequired();
            entity.HasIndex(e => e.MediaId);
        });
    }
}
