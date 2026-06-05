using MediaTracker.Modules.Media.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaTracker.Modules.Media.Services;

public class MediaService
{
    private readonly MediaDbContext MediaDb;

    public MediaService(MediaDbContext db) => MediaDb = db;

    public async Task<MediaEntry> AddAsync(string title, MediaType type, DateTime? releaseDate, WatchStatus status, int? rating)
    {
        var entry = new MediaEntry
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            MediaType = type,
            ReleaseDate = releaseDate,
            Status = status,
            Rating = rating is >= 1 and <= 10 ? rating : null,
            CreatedAt = DateTime.UtcNow
        };

        MediaDb.MediaEntries.Add(entry);
        await MediaDb.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entry = await MediaDb.MediaEntries.FindAsync(id);
        if (entry is null)
        {
            return;
        }

        MediaDb.MediaEntries.Remove(entry);
        await MediaDb.SaveChangesAsync();
    }

    public async Task UpdateTitleAsync(Guid id, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var entry = await MediaDb.MediaEntries.FindAsync(id);
        if (entry is null)
        {
            return;
        }

        entry.Title = title.Trim();
        await MediaDb.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid id, WatchStatus status)
    {
        var entry = await MediaDb.MediaEntries.FindAsync(id);
        if (entry is null)
        {
            return;
        }

        entry.Status = status;
        await MediaDb.SaveChangesAsync();
    }

    public async Task UpdateReleaseDateAsync(Guid id, DateTime? releaseDate)
    {
        var entry = await MediaDb.MediaEntries.FindAsync(id);
        if (entry is null)
        {
            return;
        }

        entry.ReleaseDate = releaseDate;
        await MediaDb.SaveChangesAsync();
    }

    public async Task UpdateRatingAsync(Guid id, int? rating)
    {
        var entry = await MediaDb.MediaEntries.FindAsync(id);
        if (entry is null)
        {
            return;
        }

        entry.Rating = rating is >= 1 and <= 10 ? rating : null;
        await MediaDb.SaveChangesAsync();
    }

    public Task<MediaEntry?> GetByIdAsync(Guid id) =>
        MediaDb.MediaEntries.FindAsync(id).AsTask();

    public async Task<List<MediaEntry>> GetListAsync(SortColumn sort, SortDirection dir, MediaType? type)
    {
        var query = MediaDb.MediaEntries.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(e => e.MediaType == type.Value);
        }

        query = (sort, dir) switch
        {
            (SortColumn.Title, SortDirection.Ascending) => query.OrderBy(e => e.Title),
            (SortColumn.Title, SortDirection.Descending) => query.OrderByDescending(e => e.Title),
            (SortColumn.Type, SortDirection.Ascending) => query.OrderBy(e => e.MediaType),
            (SortColumn.Type, SortDirection.Descending) => query.OrderByDescending(e => e.MediaType),
            (SortColumn.ReleaseDate, SortDirection.Ascending) => query.OrderBy(e => e.ReleaseDate),
            (SortColumn.ReleaseDate, SortDirection.Descending) => query.OrderByDescending(e => e.ReleaseDate),
            (SortColumn.Status, SortDirection.Ascending) => query.OrderBy(e => e.Status),
            (SortColumn.Status, SortDirection.Descending) => query.OrderByDescending(e => e.Status),
            (SortColumn.Rating, SortDirection.Ascending) => query.OrderBy(e => e.Rating),
            (SortColumn.Rating, SortDirection.Descending) => query.OrderByDescending(e => e.Rating),
            (SortColumn.Created, SortDirection.Ascending) => query.OrderBy(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.CreatedAt)
        };

        return await query.ToListAsync();
    }
}
