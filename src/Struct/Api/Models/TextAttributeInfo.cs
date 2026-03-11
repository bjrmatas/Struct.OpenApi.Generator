namespace Struct.Api.Models;

public sealed class TextAttributeInfo : AttributeInfo
{
    public override string AttributeType { get; } = "TextAttribute";
    public bool EnableAiAssistant { get; set; }
    public bool UseMultiRowInput { get; set; }
    public int? MaxLength { get; set; }
    public bool ShowCharacterCount { get; set; }
    public string? Unit { get; set; }
    public string? RegEx { get; set; }
}
