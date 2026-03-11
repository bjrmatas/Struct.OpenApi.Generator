namespace Struct.Api.Models;

public sealed class VariantGroupReferenceAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "VariantGroupReferenceAttribute";
    public bool AllowMultipleVariantGroups { get; set; }
    public bool OnlyChildren { get; set; }
    public List<string>? LimitToProductStructures { get; set; }
}
