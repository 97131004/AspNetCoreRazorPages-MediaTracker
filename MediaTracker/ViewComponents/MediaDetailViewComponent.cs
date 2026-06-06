using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MediaTracker.Modules.Media.Models;
using MediaTracker.Modules.MediaData.Models;
using MediaTracker.Pages;
using MediaTracker.Shared.UI;

using System.Threading.Tasks;

namespace MediaTracker.ViewComponents;

public class MediaDetailViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(MediaEntry media, List<MediaDataEntry> detailItems, LibraryModel model)
    {
        var viewModel = new MediaDetailViewModel
        {
            Media = media,
            DetailItems = detailItems,
            Filter = model.Filter
        };

        return Task.FromResult<IViewComponentResult>(View(viewModel));
    }
}

public class MediaDetailViewModel
{
    public MediaEntry Media { get; set; } = null!;
    public List<MediaDataEntry> DetailItems { get; set; } = new();
    public LibraryFilterOptions Filter { get; set; } = new();

    public string TagClass => Media.MediaType switch
    {
        MediaType.Movie => "tag-movie",
        MediaType.TvShow => "tag-tvshow",
        MediaType.Game => "tag-game",
        _ => ""
    };

    public string TypeLabel => Media.MediaType == MediaType.TvShow ? "TV Show" : Media.MediaType.ToString();

    public string HiddenDetailState() => Filter.ToHiddenInputsHtml() + "<input type='hidden' name='mediaId' value='" + Media.Id + "' />";

    public string CloseDetailHref()
    {
        var typePart = !string.IsNullOrEmpty(Filter.Type) ? $"&type={Filter.Type}" : "";
        return $"?sort={Filter.Sort}&dir={Filter.Dir}{typePart}";
    }
}
