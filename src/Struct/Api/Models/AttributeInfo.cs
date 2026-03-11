using System.Text.Json.Serialization;

namespace Struct.Api.Models;

[JsonConverter(typeof(AttributeInfoJsonConverter))]
public abstract class AttributeInfo : IAttributeInfo
{
    public string Uid { get; set; } = "";
    public string Alias { get; set; } = "";
    public string? BackofficeName { get; set; }
    public string? BackofficeDescription { get; set; }
    public Dictionary<string, string?>? Name { get; set; }
    public Dictionary<string, string?>? Description { get; set; }
    public bool Localized { get; set; }
    public abstract string AttributeType { get; }
    public string? DimensionUid { get; set; }
    public string? FallbackSegment { get; set; }
    public List<AttributeDimension>? Dimensions { get; set; }
    public string? GlobalListUid { get; set; }
    public bool AllowMultipleValues { get; set; }
    public bool EnableTableView { get; set; }
    public string? RenderedValueSeparator { get; set; }
    public bool Mandatory { get; set; }
}
