namespace Struct.Api.Models;

public sealed class AssetReferenceAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "AssetReferenceAttribute";
    public bool AllowMultipleAssets { get; set; }
    public List<string>? LimitToAssetTypes { get; set; }
    public string? UploadFolder { get; set; }
}
