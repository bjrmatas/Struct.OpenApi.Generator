namespace Struct.Api.Models;

public sealed class VariantReferenceAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "VariantReferenceAttribute";
    public bool AllowMultipleVariants { get; set; }
    public bool OnlyChildren { get; set; }
    public List<string>? LimitToProductStructures { get; set; }
    public List<string>? LimitToVariationDefinitions { get; set; }
}
