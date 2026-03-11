namespace Struct.Api.Models;

public sealed class NumberAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "NumberAttribute";
    public int? NumberOfDecimals { get; set; }
    public string? Unit { get; set; }
    public string? RegEx { get; set; }
    public bool IsInteger => (NumberOfDecimals ?? 0) == 0;
}
