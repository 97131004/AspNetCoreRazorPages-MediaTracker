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

        var query = MediaDb.MediaEntries.Where(e => e.Status == WatchStatus.PlanToWatch || e.Status == WatchStatus.Watching);

        if (CurrentType.HasValue)
        {
            query = query.Where(e => e.MediaType == CurrentType.Value);
        }

        var today = DateTime.Today;
        Items = await query
            // group by Status: Watching first (0), then PlanToWatch (1)
            .OrderBy(e => e.Status == WatchStatus.Watching ? 0 : 1)
            // split Timeline: future/today first (0), then past (1)
            .ThenBy(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value >= today ? 0 : 1)
            // handle future dates: ascending order (earliest future release first)
            // if its in the past, we use MaxValue so it stays at the bottom of this specific sort tier
            .ThenBy(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value >= today ? e.ReleaseDate.Value : DateTime.MaxValue)
            // handle past dates: descending order (most recent past release first)
            // if its in the future, we use MinValue so it doesn't interfere with this sort tier
            .ThenByDescending(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value < today ? e.ReleaseDate.Value : DateTime.MinValue)
            // tie-breaker: alphabetical by title
            .ThenBy(e => e.Title)
            .ToListAsync();
    }

    public string TypeHref(string? type)
    {
        var typeParam = type != null ? $"?type={type}" : "/";
        return typeParam;
    }
}
