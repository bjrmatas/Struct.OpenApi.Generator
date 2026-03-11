using System.Text.Json;

namespace Struct.Api.Models;

public sealed class AttributeInfoDetection
{
    public required string AttributeType { get; init; }
    public AttributeInfo GetAttributeInfo(
        string data,
        JsonSerializerOptions? options = null
    ) => AttributeType?.ToLowerInvariant() switch
    {
        "textattribute" => JsonSerializer.Deserialize<TextAttributeInfo>(data, options)!,
        "numberattribute" => JsonSerializer.Deserialize<NumberAttributeInfo>(data, options)!,
        "booleanattribute" => JsonSerializer.Deserialize<BooleanAttributeInfo>(data, options)!,
        "datetimeattribute" => JsonSerializer.Deserialize<DateTimeAttributeInfo>(data, options)!,
        "assetreferenceattribute" => JsonSerializer.Deserialize<AssetReferenceAttributeInfo>(data, options)!,
        "categoryreferenceattribute" => JsonSerializer.Deserialize<CategoryReferenceAttributeInfo>(data, options)!,
        "productreferenceattribute" => JsonSerializer.Deserialize<ProductReferenceAttributeInfo>(data, options)!,
        "variantreferenceattribute" => JsonSerializer.Deserialize<VariantReferenceAttributeInfo>(data, options)!,
        "variantgroupreferenceattribute" => JsonSerializer.Deserialize<VariantGroupReferenceAttributeInfo>(data, options)!,
        "listattribute" => JsonSerializer.Deserialize<ListAttributeInfo>(data, options)!,
        "fixedlistattribute" => JsonSerializer.Deserialize<FixedListAttributeInfo>(data, options)!,
        _ when AttributeType?.Contains("complex", StringComparison.OrdinalIgnoreCase) == true
            => JsonSerializer.Deserialize<ComplexAttributeInfo>(data, options)!,
        _ => JsonSerializer.Deserialize<TextAttributeInfo>(data, options)!
    };
}
