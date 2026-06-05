namespace MediaTracker.Modules.Media.Models;

public class MediaEntry
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public WatchStatus Status { get; set; }
    public int? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
