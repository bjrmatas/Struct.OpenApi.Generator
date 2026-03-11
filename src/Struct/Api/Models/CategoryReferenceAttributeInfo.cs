namespace Struct.Api.Models;

public sealed class CategoryReferenceAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "CategoryReferenceAttribute";
    public bool AllowMultipleCategories { get; set; }
    public List<string>? LimitToCatalogues { get; set; }
}
