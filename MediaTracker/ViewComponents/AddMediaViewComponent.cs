using System;
using Microsoft.AspNetCore.Mvc;
using MediaTracker.Pages;
using MediaTracker.Modules.Media.Models;
using MediaTracker.Shared.UI;

using System.Threading.Tasks;

namespace MediaTracker.ViewComponents;

public class AddMediaViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(LibraryModel model)
    {
        var viewModel = new AddMediaViewModel
        {
            Filter = model.Filter,
            NewTitle = model.NewTitle,
            NewType = model.NewType,
            NewReleaseDate = model.NewReleaseDate,
            NewStatus = model.NewStatus,
            NewRating = model.NewRating
        };

        return Task.FromResult<IViewComponentResult>(View(viewModel));
    }
}

public class AddMediaViewModel
{
    public LibraryFilterOptions Filter { get; set; } = new();

    public string NewTitle { get; set; } = string.Empty;
    public MediaType? NewType { get; set; }
    public DateTime? NewReleaseDate { get; set; }
    public WatchStatus NewStatus { get; set; } = WatchStatus.PlanToWatch;
    public int? NewRating { get; set; }
}
