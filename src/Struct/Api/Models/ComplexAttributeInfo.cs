namespace Struct.Api.Models;

public sealed class ComplexAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "ComplexAttribute";
    public List<AttributeInfo>? SubAttributes { get; set; }
}