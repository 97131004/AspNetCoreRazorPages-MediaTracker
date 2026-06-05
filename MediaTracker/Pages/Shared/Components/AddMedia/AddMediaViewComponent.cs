using System;
using Microsoft.AspNetCore.Mvc;
using MediaTracker.Pages;
using MediaTracker.Modules.Media.Models;

using System.Threading.Tasks;

namespace MediaTracker.Pages.Shared.Components;

public class AddMediaViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync(LibraryModel model)
    {
        var viewModel = new AddMediaViewModel
        {
            CurrentSort = model.CurrentSort,
            CurrentDir = model.CurrentDir,
            CurrentType = model.CurrentType,
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
    public SortColumn CurrentSort { get; set; } = SortColumn.Created;
    public SortDirection CurrentDir { get; set; } = SortDirection.Descending;
    public MediaType? CurrentType { get; set; }

    public string NewTitle { get; set; } = string.Empty;
    public MediaType? NewType { get; set; }
    public DateTime? NewReleaseDate { get; set; }
    public WatchStatus NewStatus { get; set; } = WatchStatus.PlanToWatch;
    public int? NewRating { get; set; }
}
