using MediaTracker.Modules.MediaData.Models;

namespace MediaTracker.Modules.MediaData.Services;

public interface IMediaDataService
{
    Task<List<MediaDataEntry>> GetByMediaIdAsync(Guid mediaId);

    Task<List<MediaDataEntry>> GetByMediaIdsAsync(IEnumerable<Guid> mediaIds);

    Task<MediaDataEntry> AddAsync(Guid mediaId, string dataType, string value);

    Task UpdateAsync(Guid id, string dataType, string value);

    Task<MediaDataEntry?> DeleteAsync(Guid id);

    Task DeleteAllForMediaAsync(Guid mediaId);
}
