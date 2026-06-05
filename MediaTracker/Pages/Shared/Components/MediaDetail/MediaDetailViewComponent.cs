using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MediaTracker.Modules.Media.Models;
using MediaTracker.Modules.MediaData.Models;
using MediaTracker.Pages;

using System.Threading.Tasks;

namespace MediaTracker.Pages.Shared.Components;

public class MediaDetailViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(MediaEntry media, List<MediaDataEntry> detailItems, LibraryModel model)
    {
        var viewModel = new MediaDetailViewModel
        {
            Media = media,
            DetailItems = detailItems,
            CurrentSort = model.CurrentSort,
            CurrentDir = model.CurrentDir,
            CurrentType = model.CurrentType
        };

        return Task.FromResult<IViewComponentResult>(View(viewModel));
    }
}

public class MediaDetailViewModel
{
    public MediaEntry Media { get; set; } = null!;
    public List<MediaDataEntry> DetailItems { get; set; } = new();
    public SortColumn CurrentSort { get; set; } = SortColumn.Created;
    public SortDirection CurrentDir { get; set; } = SortDirection.Descending;
    public MediaType? CurrentType { get; set; }

    public string TagClass => Media.MediaType switch
    {
        MediaType.Movie => "tag-movie",
        MediaType.TvShow => "tag-tvshow",
        MediaType.Game => "tag-game",
        _ => ""
    };

    public string TypeLabel => Media.MediaType == MediaType.TvShow ? "TV Show" : Media.MediaType.ToString();

    public string HiddenState() =>
        "<input type='hidden' name='sort' value='" + CurrentSort + "' />" +
        "<input type='hidden' name='dir' value='" + CurrentDir + "' />" +
        (CurrentType.HasValue ? "<input type='hidden' name='type' value='" + CurrentType.Value.ToString().ToLower() + "' />" : "");

    public string HiddenDetailState() => HiddenState() + "<input type='hidden' name='detailId' value='" + Media.Id + "' />";

    public string CloseDetailHref()
    {
        var typePart = CurrentType.HasValue ? $"&type={CurrentType.Value.ToString().ToLower()}" : "";
        return $"?sort={CurrentSort}&dir={CurrentDir}{typePart}";
    }
}
