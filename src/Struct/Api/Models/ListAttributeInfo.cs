namespace Struct.Api.Models;

public sealed class ListAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "ListAttribute";
    public string? ItemType { get; set; }
}
