namespace Struct.Api.Models;

public sealed record AttributeInfo(
    string Uid,
    string Alias,
    string? BackofficeName,
    Dictionary<string, string>? Name,
    Dictionary<string, string>? Description,
    bool Localized,
    string? AttributeType,
    string? DimensionUid,
    List<AttributeDimension>? Dimensions);

public sealed record AttributeDimension(string Uid, string Alias, string Name, string? CultureCode, string? Segment);
