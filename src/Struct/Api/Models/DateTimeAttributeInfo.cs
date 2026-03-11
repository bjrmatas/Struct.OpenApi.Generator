namespace Struct.Api.Models;

public sealed class DateTimeAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "DateTimeAttribute";
    public bool IncludeTime { get; set; }
}
