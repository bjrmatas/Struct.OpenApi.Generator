namespace Struct.Api.Models;

public sealed class FixedListAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "FixedListAttribute";
    public AttributeInfo? Template { get; set; }
    public AttributeInfo? ReferencedAttribute { get; set; }
}
