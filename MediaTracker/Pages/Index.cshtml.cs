using MediaTracker.Modules.Media;
using MediaTracker.Modules.Media.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediaTracker.Pages;

public class IndexModel : PageModel
{
    private readonly MediaDbContext MediaDb;

    public IndexModel(MediaDbContext db) => MediaDb = db;

    public List<MediaEntry> Items { get; set; } = new();
    public MediaType? CurrentType { get; set; }

    public async Task OnGetAsync(string? type)
    {
        CurrentType = type?.ToLower() switch
        {
            "movie" => MediaType.Movie,
            "tvshow" => MediaType.TvShow,
            "game" => MediaType.Game,
            _ => null
        };

        var query = MediaDb.MediaEntries.Where(e => e.Status == WatchStatus.PlanToWatch);

        if (CurrentType.HasValue)
        {
            query = query.Where(e => e.MediaType == CurrentType.Value);
        }

        var today = DateTime.Today;
        // for dates in future: sort by ReleaseDate ascendingly
        // for dates in past: sort by ReleaseDate descendingly
        Items = await query
            .OrderBy(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value >= today ? 0 : 1)
            .ThenBy(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value >= today
                ? e.ReleaseDate.Value
                : DateTime.MaxValue)
            .ThenByDescending(e => e.ReleaseDate)
            .ThenBy(e => e.Title)
            .ToListAsync();
    }

    public string TypeHref(string? type)
    {
        var typeParam = type != null ? $"?type={type}" : "/";
        return typeParam;
    }
}
