using MediaTracker.Modules.Media.Models;

namespace MediaTracker.Shared.UI;

public class LibraryFilterOptions
{
    public string? Sort { get; set; }
    public string? Dir { get; set; }
    public string? Type { get; set; }

    public SortColumn GetActiveSort()
    {
        return Enum.TryParse<SortColumn>(Sort, true, out var sortCol) ? sortCol : SortColumn.Created;
    }

    public SortDirection GetActiveDir()
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

    public MediaType? GetActiveType() => Type?.ToLowerInvariant() switch
    {
        "movie" => MediaType.Movie,
        "tvshow" or "tv_show" => MediaType.TvShow,
        "game" => MediaType.Game,
        _ => null
    };

    public string ToHiddenInputsHtml()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var prop in GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (prop.CanRead && prop.CanWrite && (prop.PropertyType == typeof(string) || prop.PropertyType.IsValueType))
            {
                var val = prop.GetValue(this);
                if (val != null)
                {
                    sb.Append($"<input type='hidden' name='{prop.Name}' value='{System.Text.Encodings.Web.HtmlEncoder.Default.Encode(val.ToString()!)}' />");
                }
            }
        }
        return sb.ToString();
    }
}
