using System.ComponentModel.DataAnnotations;
using MediaTracker.Modules.Media.Models;
using MediaTracker.Modules.Media.Services;
using MediaTracker.Modules.MediaData.Models;
using MediaTracker.Modules.MediaData.Services;
using MediaTracker.Shared.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

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

    public bool ShowAddDialog { get; set; }

    // detail dialog

    public Guid? MediaId { get; set; }
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
        var activeSort = Filter.GetActiveSort();
        var activeDir = Filter.GetActiveDir();
        var newDir = (activeSort == col && activeDir == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={col}&dir={newDir}{typePart}";
    }

    public string SortIndicator(SortColumn col) => Filter.GetActiveSort() == col ? (Filter.GetActiveDir() == SortDirection.Ascending ? " ↑" : " ↓") : "";

    public string TypeHref(string? type)
    {
        var typePart = type != null ? $"type={type}&" : "";
        return $"?{typePart}sort={Filter.GetActiveSort()}&dir={Filter.GetActiveDir()}";
    }

    public IReadOnlyList<MediaDataEntry> GetItemData(Guid id)
    {
        return ItemData.TryGetValue(id, out var list) ? list : Array.Empty<MediaDataEntry>();
    }

    // link that opens the detail dialog for a specific media item
    public string DetailHref(Guid id)
    {
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={Filter.GetActiveSort()}&dir={Filter.GetActiveDir()}{typePart}&mediaId={id}";
    }

    // link that closes the detail dialog (same url without detail param)
    public string CloseDetailHref()
    {
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={Filter.GetActiveSort()}&dir={Filter.GetActiveDir()}{typePart}";
    }

    // handlers

    public async Task OnGetAsync(Guid? mediaId)
    {
        await LoadAsync();
        MediaId = mediaId;
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

        return RedirectToPage(Filter);
    }

    // library inline edits

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var entry = await MediaService.GetByIdAsync(id);
        var title = entry?.Title ?? "Item";
        await MediaService.DeleteAsync(id);
        await MediaDataService.DeleteAllForMediaAsync(id);
        TempData["Removed"] = $"\"{title}\" removed.";
        return RedirectToPage(Filter);
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

        return RedirectToPage(Filter);
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
        return RedirectToPage(Filter);
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
        return RedirectToPage(Filter);
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
        return RedirectToPage(Filter);
    }

    // detail dialog: media data crud

    public async Task<IActionResult> OnPostDetailAddAsync(Guid mediaId, string? newDataType, string? newDataValue)
    {
        if (!string.IsNullOrWhiteSpace(newDataType) && !string.IsNullOrWhiteSpace(newDataValue))
        {
            await MediaDataService.AddAsync(mediaId, newDataType, newDataValue);
            var entry = await MediaService.GetByIdAsync(mediaId);
            var title = entry?.Title ?? "Item";
            TempData["Success"] = $"Added \"{newDataType.Trim()}: {newDataValue.Trim()}\" to \"{title}\".";
        }

        return RedirectToDetail(mediaId);
    }

    public async Task<IActionResult> OnPostDetailUpdateAsync(Guid mediaId, Guid detailId, string? dataType, string? value)
    {
        if (!string.IsNullOrWhiteSpace(dataType) && !string.IsNullOrWhiteSpace(value))
        {
            await MediaDataService.UpdateAsync(detailId, dataType, value);
            var entry = await MediaService.GetByIdAsync(mediaId);
            var title = entry?.Title ?? "Item";
            TempData["Success"] = $"Updated detail for \"{title}\" to \"{dataType.Trim()}: {value.Trim()}\".";
        }

        return RedirectToDetail(mediaId);
    }

    public async Task<IActionResult> OnPostDetailDeleteAsync(Guid mediaId, Guid detailId)
    {
        var dataEntry = await MediaDataService.DeleteAsync(detailId);
        var label = dataEntry is not null ? $"\"{dataEntry.DataType}: {dataEntry.Value}\"" : "detail";
        var entry = await MediaService.GetByIdAsync(mediaId);
        var title = entry?.Title ?? "Item";
        TempData["Removed"] = $"Removed {label} from \"{title}\".";
        return RedirectToDetail(mediaId);
    }

    // private helpers

    private async Task LoadAsync()
    {
        Items = await MediaService.GetListAsync(Filter.GetActiveSort(), Filter.GetActiveDir(), Filter.GetActiveType());
        var allData = await MediaDataService.GetByMediaIdsAsync(Items.Select(i => i.Id));
        ItemData = allData.GroupBy(d => d.MediaId).ToDictionary(g => g.Key, g => g.ToList());
    }

    private IActionResult RedirectToDetail(Guid mediaId)
    {
        var routeValues = new RouteValueDictionary(Filter)
        {
            ["mediaId"] = mediaId
        };
        return RedirectToPage(routeValues);
    }
}
