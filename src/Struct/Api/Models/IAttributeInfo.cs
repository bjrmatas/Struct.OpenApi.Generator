namespace Struct.Api.Models;

public interface IAttributeInfo
{
    string Uid { get; }
    string Alias { get; }
    string? BackofficeName { get; }
    string? BackofficeDescription { get; }
    Dictionary<string, string?>? Name { get; }
    Dictionary<string, string?>? Description { get; }
    bool Localized { get; }
    string? DimensionUid { get; }
    string? FallbackSegment { get; }
    bool AllowMultipleValues { get; }
    bool Mandatory { get; }
    string AttributeType { get; }
}
