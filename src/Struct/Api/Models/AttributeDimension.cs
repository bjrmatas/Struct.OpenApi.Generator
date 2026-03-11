namespace Struct.Api.Models;

public sealed class AttributeDimension
{
    public string Uid { get; set; } = "";
    public string Alias { get; set; } = "";
    public string Name { get; set; } = "";
    public string? CultureCode { get; set; }
    public string? Segment { get; set; }
}
