namespace MediaTracker.Modules.MediaData.Models;

public class MediaDataEntry
{
    public Guid Id { get; set; }
    public Guid MediaId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
