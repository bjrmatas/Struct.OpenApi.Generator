namespace Struct.Api.Models;

public sealed class ProductReferenceAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "ProductReferenceAttribute";
    public bool AllowMultipleProducts { get; set; }
    public List<string>? LimitToProductStructures { get; set; }
}
