using MediaTracker.Modules.Media.Models;
using MediaTracker.Shared.UI;

namespace MediaTracker.Modules.Media.Services;

public interface IMediaService
{
    Task<MediaEntry> AddAsync(string title, MediaType type, DateTime? releaseDate, WatchStatus status, int? rating);

    Task DeleteAsync(Guid id);

    Task UpdateTitleAsync(Guid id, string title);

    Task UpdateStatusAsync(Guid id, WatchStatus status);

    Task UpdateReleaseDateAsync(Guid id, DateTime? releaseDate);

    Task UpdateRatingAsync(Guid id, int? rating);

    Task<MediaEntry?> GetByIdAsync(Guid id);

    Task<List<MediaEntry>> GetListAsync(SortColumn sort, SortDirection dir, MediaType? type);
}
