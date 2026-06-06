using System.ComponentModel.DataAnnotations;
using MediaTracker.Modules.Media.Models;
using MediaTracker.Modules.Media.Services;
using MediaTracker.Modules.MediaData.Models;
using MediaTracker.Modules.MediaData.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediaTracker.Pages;

public class LibraryModel : PageModel
{
    private readonly MediaService MediaService;
    private readonly MediaDataService MediaDataService;

    public LibraryModel(MediaService mediaService, MediaDataService mediaDataService)
    {
        MediaService = mediaService;
        MediaDataService = mediaDataService;
    }

    // library list
    public List<MediaEntry> Items { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public LibraryFilterOptions Filter { get; set; } = new();

    public SortColumn CurrentSort => Filter.ActiveSort;
    public SortDirection CurrentDir => Filter.ActiveDir;
    public MediaType? CurrentType => Filter.ActiveType;

    public bool ShowAddDialog { get; set; }

    // detail dialog

    public Guid? DetailId { get; set; }
    public Dictionary<Guid, List<MediaDataEntry>> ItemData { get; set; } = new();

    // add media form

    [BindProperty]
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500)]
    public string NewTitle { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Please select a type.")]
    public MediaType? NewType { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime? NewReleaseDate { get; set; }

    [BindProperty]
    public WatchStatus NewStatus { get; set; } = WatchStatus.PlanToWatch;

    [BindProperty]
    public int? NewRating { get; set; }

    // url helpers

    public string SortHref(SortColumn col)
    {
        var newDir = (CurrentSort == col && CurrentDir == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={col}&dir={newDir}{typePart}";
    }

    public string SortIndicator(SortColumn col) => CurrentSort == col ? (CurrentDir == SortDirection.Ascending ? " ↑" : " ↓") : "";

    public string TypeHref(string? type)
    {
        var typePart = type != null ? $"type={type}&" : "";
        return $"?{typePart}sort={CurrentSort}&dir={CurrentDir}";
    }

    public IReadOnlyList<MediaDataEntry> GetItemData(Guid id)
    {
        return ItemData.TryGetValue(id, out var list) ? list : Array.Empty<MediaDataEntry>();
    }

    // link that opens the detail dialog for a specific media item
    public string DetailHref(Guid id)
    {
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={CurrentSort}&dir={CurrentDir}{typePart}&detail={id}";
    }

    // link that closes the detail dialog (same url without detail param)
    public string CloseDetailHref()
    {
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={CurrentSort}&dir={CurrentDir}{typePart}";
    }

    // handlers

    public async Task OnGetAsync(Guid? detail)
    {
        await LoadAsync();
        DetailId = detail;
    }

    // add media

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            ShowAddDialog = true;
            return Page();
        }

        try
        {
            var entry = await MediaService.AddAsync(NewTitle, NewType!.Value, NewReleaseDate, NewStatus, NewRating);
            TempData["Success"] = $"\"{entry.Title}\" added.";
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
            await LoadAsync();
            ShowAddDialog = true;
            return Page();
        }

        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type });
    }

    // library inline edits

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var entry = await MediaService.GetByIdAsync(id);
        var title = entry?.Title ?? "Item";
        await MediaService.DeleteAsync(id);
        await MediaDataService.DeleteAllForMediaAsync(id);
        TempData["Removed"] = $"\"{title}\" removed.";
        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type });
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, int newStatus)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null && Enum.IsDefined(typeof(WatchStatus), newStatus))
        {
            await MediaService.UpdateStatusAsync(id, (WatchStatus)newStatus);
            var statusLabel = (WatchStatus)newStatus switch
            {
                WatchStatus.PlanToWatch => entry.MediaType == MediaType.Game ? "Plan to Play" : "Plan to Watch",
                WatchStatus.Watching => entry.MediaType == MediaType.Game ? "Playing" : "Watching",
                WatchStatus.Completed => entry.MediaType == MediaType.Game ? "Played" : "Completed",
                _ => ((WatchStatus)newStatus).ToString()
            };
            TempData["Success"] = $"\"{entry.Title}\" status updated to {statusLabel}.";
        }

        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type });
    }

    public async Task<IActionResult> OnPostUpdateReleaseDateAsync(Guid id, DateTime? newReleaseDate)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null)
        {
            await MediaService.UpdateReleaseDateAsync(id, newReleaseDate);
            var dateStr = newReleaseDate?.ToString("yyyy-MM-dd") ?? "none";
            TempData["Success"] = $"\"{entry.Title}\" release date updated to {dateStr}.";
        }
        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type });
    }

    public async Task<IActionResult> OnPostUpdateTitleAsync(Guid id, string newTitle)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null)
        {
            var oldTitle = entry.Title;
            await MediaService.UpdateTitleAsync(id, newTitle);
            TempData["Success"] = $"\"{oldTitle}\" title updated to \"{newTitle.Trim()}\".";
        }
        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type });
    }

    public async Task<IActionResult> OnPostUpdateRatingAsync(Guid id, int? rating)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null)
        {
            await MediaService.UpdateRatingAsync(id, rating);
            var ratingStr = rating.HasValue ? $"{rating}/10" : "none";
            TempData["Success"] = $"\"{entry.Title}\" rating updated to {ratingStr}.";
        }
        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type });
    }

    // detail dialog: media data crud

    public async Task<IActionResult> OnPostDetailAddAsync(Guid detailId, string? newDataType, string? newDataValue)
    {
        if (!string.IsNullOrWhiteSpace(newDataType) && !string.IsNullOrWhiteSpace(newDataValue))
        {
            await MediaDataService.AddAsync(detailId, newDataType, newDataValue);
            var entry = await MediaService.GetByIdAsync(detailId);
            var title = entry?.Title ?? "Item";
            TempData["Success"] = $"Added \"{newDataType.Trim()}: {newDataValue.Trim()}\" to \"{title}\".";
        }

        return RedirectToDetail(detailId);
    }

    public async Task<IActionResult> OnPostDetailUpdateAsync(Guid detailId, Guid entryId, string? dataType, string? value)
    {
        if (!string.IsNullOrWhiteSpace(dataType) && !string.IsNullOrWhiteSpace(value))
        {
            await MediaDataService.UpdateAsync(entryId, dataType, value);
            var entry = await MediaService.GetByIdAsync(detailId);
            var title = entry?.Title ?? "Item";
            TempData["Success"] = $"Updated detail for \"{title}\" to \"{dataType.Trim()}: {value.Trim()}\".";
        }

        return RedirectToDetail(detailId);
    }

    public async Task<IActionResult> OnPostDetailDeleteAsync(Guid detailId, Guid entryId)
    {
        var dataEntry = await MediaDataService.DeleteAsync(entryId);
        var label = dataEntry is not null ? $"\"{dataEntry.DataType}: {dataEntry.Value}\"" : "detail";
        var entry = await MediaService.GetByIdAsync(detailId);
        var title = entry?.Title ?? "Item";
        TempData["Removed"] = $"Removed {label} from \"{title}\".";
        return RedirectToDetail(detailId);
    }

    // private helpers

    private async Task LoadAsync()
    {
        Items = await MediaService.GetListAsync(CurrentSort, CurrentDir, CurrentType);
        var allData = await MediaDataService.GetByMediaIdsAsync(Items.Select(i => i.Id));
        ItemData = allData.GroupBy(d => d.MediaId).ToDictionary(g => g.Key, g => g.ToList());
    }

    private IActionResult RedirectToDetail(Guid detailId)
    {
        return RedirectToPage(new { sort = Filter.Sort, dir = Filter.Dir, type = Filter.Type, detail = detailId });
    }
}

public class LibraryFilterOptions
{
    public string? Sort { get; set; }
    public string? Dir { get; set; }
    public string? Type { get; set; }

    public SortColumn ActiveSort => Enum.TryParse<SortColumn>(Sort, true, out var sortCol) ? sortCol : SortColumn.Created;

    public SortDirection ActiveDir
    {
        get
        {
            if (Enum.TryParse<SortDirection>(Dir, true, out var sortDir))
            {
                return sortDir;
            }
            if (Dir?.ToLowerInvariant() == "asc")
            {
                return SortDirection.Ascending;
            }
            return SortDirection.Descending;
        }
    }

    public MediaType? ActiveType => Type?.ToLowerInvariant() switch
    {
        "movie" => MediaType.Movie,
        "tvshow" or "tv_show" => MediaType.TvShow,
        "game" => MediaType.Game,
        _ => null
    };
}
