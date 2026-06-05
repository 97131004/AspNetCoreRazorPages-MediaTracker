using MediaTracker.Modules.MediaData.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaTracker.Modules.MediaData.Services;

public class MediaDataService
{
    private readonly MediaDataDbContext MediaDataDb;

    public MediaDataService(MediaDataDbContext db) => MediaDataDb = db;

    public Task<List<MediaDataEntry>> GetByMediaIdAsync(Guid mediaId) =>
        MediaDataDb.MediaDataEntries
           .Where(e => e.MediaId == mediaId)
           .OrderBy(e => e.DataType)
           .ToListAsync();

    public Task<List<MediaDataEntry>> GetByMediaIdsAsync(IEnumerable<Guid> mediaIds) =>
        MediaDataDb.MediaDataEntries
           .Where(e => mediaIds.Contains(e.MediaId))
           .OrderBy(e => e.DataType)
           .ToListAsync();

    public async Task<MediaDataEntry> AddAsync(Guid mediaId, string dataType, string value)
    {
        var entry = new MediaDataEntry
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            DataType = dataType.Trim().ToLowerInvariant(),
            Value = value.Trim()
        };

        MediaDataDb.MediaDataEntries.Add(entry);
        await MediaDataDb.SaveChangesAsync();
        return entry;
    }

    public async Task UpdateAsync(Guid id, string dataType, string value)
    {
        var entry = await MediaDataDb.MediaDataEntries.FindAsync(id);
        if (entry is null)
        {
            return;
        }

        entry.DataType = dataType.Trim().ToLowerInvariant();
        entry.Value = value.Trim();
        await MediaDataDb.SaveChangesAsync();
    }

    public async Task<MediaDataEntry?> DeleteAsync(Guid id)
    {
        var entry = await MediaDataDb.MediaDataEntries.FindAsync(id);
        if (entry is null)
        {
            return null;
        }

        MediaDataDb.MediaDataEntries.Remove(entry);
        await MediaDataDb.SaveChangesAsync();
        return entry;
    }

    // remove all data entries for a media item (call when deleting a media entry)
    public async Task DeleteAllForMediaAsync(Guid mediaId)
    {
        var entries = await MediaDataDb.MediaDataEntries.Where(e => e.MediaId == mediaId).ToListAsync();
        MediaDataDb.MediaDataEntries.RemoveRange(entries);
        await MediaDataDb.SaveChangesAsync();
    }
}
