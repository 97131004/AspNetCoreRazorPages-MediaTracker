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
    public SortColumn CurrentSort { get; set; } = SortColumn.Created;
    public SortDirection CurrentDir { get; set; } = SortDirection.Descending;
    public MediaType? CurrentType { get; set; }
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
        var typePart = CurrentType.HasValue ? $"&type={CurrentType.Value.ToString().ToLower()}" : "";
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
        var typePart = CurrentType.HasValue ? $"&type={CurrentType.Value.ToString().ToLower()}" : "";
        return $"?sort={CurrentSort}&dir={CurrentDir}{typePart}&detail={id}";
    }

    // link that closes the detail dialog (same url without detail param)
    public string CloseDetailHref()
    {
        var typePart = CurrentType.HasValue ? $"&type={CurrentType.Value.ToString().ToLower()}" : "";
        return $"?sort={CurrentSort}&dir={CurrentDir}{typePart}";
    }

    // handlers

    public async Task OnGetAsync(string? sort, string? dir, string? type, Guid? detail)
    {
        ParseView(sort, dir, type);
        await LoadAsync();
        DetailId = detail;
    }

    // add media

    public async Task<IActionResult> OnPostAddAsync(string? sort, string? dir, string? type)
    {
        ParseView(sort, dir, type);

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

        return RedirectToPage(new { sort = CurrentSort, dir = CurrentDir, type = CurrentType?.ToString().ToLower() });
    }

    // library inline edits

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, string? sort, string? dir, string? type)
    {
        var entry = await MediaService.GetByIdAsync(id);
        var title = entry?.Title ?? "Item";
        await MediaService.DeleteAsync(id);
        await MediaDataService.DeleteAllForMediaAsync(id);
        TempData["Removed"] = $"\"{title}\" removed.";
        return RedirectToPage(new { sort, dir, type });
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, int newStatus, string? sort, string? dir, string? type)
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

        return RedirectToPage(new { sort, dir, type });
    }

    public async Task<IActionResult> OnPostUpdateReleaseDateAsync(Guid id, DateTime? newReleaseDate, string? sort, string? dir, string? type)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null)
        {
            await MediaService.UpdateReleaseDateAsync(id, newReleaseDate);
            var dateStr = newReleaseDate?.ToString("yyyy-MM-dd") ?? "none";
            TempData["Success"] = $"\"{entry.Title}\" release date updated to {dateStr}.";
        }
        return RedirectToPage(new { sort, dir, type });
    }

    public async Task<IActionResult> OnPostUpdateTitleAsync(Guid id, string newTitle, string? sort, string? dir, string? type)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null)
        {
            var oldTitle = entry.Title;
            await MediaService.UpdateTitleAsync(id, newTitle);
            TempData["Success"] = $"\"{oldTitle}\" title updated to \"{newTitle.Trim()}\".";
        }
        return RedirectToPage(new { sort, dir, type });
    }

    public async Task<IActionResult> OnPostUpdateRatingAsync(Guid id, int? rating, string? sort, string? dir, string? type)
    {
        var entry = await MediaService.GetByIdAsync(id);
        if (entry is not null)
        {
            await MediaService.UpdateRatingAsync(id, rating);
            var ratingStr = rating.HasValue ? $"{rating}/10" : "none";
            TempData["Success"] = $"\"{entry.Title}\" rating updated to {ratingStr}.";
        }
        return RedirectToPage(new { sort, dir, type });
    }

    // detail dialog: media data crud

    public async Task<IActionResult> OnPostDetailAddAsync(Guid detailId, string? newDataType, string? newDataValue, string? sort, string? dir, string? type)
    {
        ParseView(sort, dir, type);

        if (!string.IsNullOrWhiteSpace(newDataType) && !string.IsNullOrWhiteSpace(newDataValue))
        {
            await MediaDataService.AddAsync(detailId, newDataType, newDataValue);
            var entry = await MediaService.GetByIdAsync(detailId);
            var title = entry?.Title ?? "Item";
            TempData["Success"] = $"Added \"{newDataType.Trim()}: {newDataValue.Trim()}\" to \"{title}\".";
        }

        return RedirectToDetail(detailId);
    }

    public async Task<IActionResult> OnPostDetailUpdateAsync(Guid detailId, Guid entryId, string? dataType, string? value, string? sort, string? dir, string? type)
    {
        ParseView(sort, dir, type);

        if (!string.IsNullOrWhiteSpace(dataType) && !string.IsNullOrWhiteSpace(value))
        {
            await MediaDataService.UpdateAsync(entryId, dataType, value);
            var entry = await MediaService.GetByIdAsync(detailId);
            var title = entry?.Title ?? "Item";
            TempData["Success"] = $"Updated detail for \"{title}\" to \"{dataType.Trim()}: {value.Trim()}\".";
        }

        return RedirectToDetail(detailId);
    }

    public async Task<IActionResult> OnPostDetailDeleteAsync(Guid detailId, Guid entryId, string? sort, string? dir, string? type)
    {
        ParseView(sort, dir, type);
        var dataEntry = await MediaDataService.DeleteAsync(entryId);
        var label = dataEntry is not null ? $"\"{dataEntry.DataType}: {dataEntry.Value}\"" : "detail";
        var entry = await MediaService.GetByIdAsync(detailId);
        var title = entry?.Title ?? "Item";
        TempData["Removed"] = $"Removed {label} from \"{title}\".";
        return RedirectToDetail(detailId);
    }

    // private helpers

    private void ParseView(string? sort, string? dir, string? type)
    {
        if (Enum.TryParse<SortColumn>(sort, true, out var sortCol))
        {
            CurrentSort = sortCol;
        }
        else
        {
            CurrentSort = SortColumn.Created;
        }

        if (Enum.TryParse<SortDirection>(dir, true, out var sortDirection))
        {
            CurrentDir = sortDirection;
        }
        else if (dir?.ToLowerInvariant() == "asc")
        {
            CurrentDir = SortDirection.Ascending;
        }
        else if (dir?.ToLowerInvariant() == "desc")
        {
            CurrentDir = SortDirection.Descending;
        }
        else
        {
            CurrentDir = SortDirection.Descending;
        }

        CurrentType = type?.ToLower() switch
        {
            "movie" => MediaType.Movie,
            "tvshow" => MediaType.TvShow,
            "game" => MediaType.Game,
            _ => null
        };
    }

    private async Task LoadAsync()
    {
        Items = await MediaService.GetListAsync(CurrentSort, CurrentDir, CurrentType);
        var allData = await MediaDataService.GetByMediaIdsAsync(Items.Select(i => i.Id));
        ItemData = allData.GroupBy(d => d.MediaId).ToDictionary(g => g.Key, g => g.ToList());
    }

    private IActionResult RedirectToDetail(Guid detailId)
    {
        var typePart = CurrentType.HasValue ? CurrentType.Value.ToString().ToLower() : null;
        return RedirectToPage(new { sort = CurrentSort, dir = CurrentDir, type = typePart, detail = detailId });
    }
}
