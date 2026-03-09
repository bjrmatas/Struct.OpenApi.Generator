namespace Struct.Api.Models;

public class Dimension
{
    public required string Uid { get; set; }
    public required string Alias { get; set; }
    public List<DimensionSegment> Segments { get; set; } = [];
}

public class DimensionSegment
{
    public required string Uid { get; set; }
    public required string Identifier { get; set; }
    public required string Name { get; set; }
}

public class Language
{
    public required string CultureCode { get; set; }
    public required string Name { get; set; }
    public bool IsDefault { get; set; }
}
