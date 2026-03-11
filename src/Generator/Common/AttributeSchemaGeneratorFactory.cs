using Struct.Api.Models;
using Struct.OpenApi.Generator.Services.Generators;

namespace Struct.OpenApi.Generator.Common;

public static class AttributeSchemaGeneratorFactory
{
    private static readonly Dictionary<string, Func<IAttributeSchemaGenerator>> Generators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["textattribute"] = () => new PrimitiveAttributeSchemaGenerator("string", null, () => "sample text"),
        ["numberattribute"] = () => new PrimitiveAttributeSchemaGenerator("number", null, () => 42.5),
        ["booleanattribute"] = () => new PrimitiveAttributeSchemaGenerator("boolean", null, () => true),
        ["datetimeattribute"] = () => new PrimitiveAttributeSchemaGenerator("string", "date-time", () => "2024-01-15T10:30:00Z"),
        ["assetreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32", () => 1),
        ["categoryreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32", () => 1),
        ["productreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32", () => 1),
        ["variantreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32", () => 1),
        ["variantgroupreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32", () => 1),
        ["listattribute"] = () => new ListAttributeSchemaGenerator(),
        ["fixedlistattribute"] = () => new FixedListAttributeSchemaGenerator(),
    };

    public static IAttributeSchemaGenerator GetGenerator(string? attributeType)
    {
        if (string.IsNullOrEmpty(attributeType))
        {
            return new PrimitiveAttributeSchemaGenerator("string", null, () => "sample text");
        }

        if (attributeType.Contains("complex", StringComparison.OrdinalIgnoreCase))
        {
            return new ComplexAttributeSchemaGenerator();
        }

        return Generators.GetValueOrDefault(attributeType, () => new PrimitiveAttributeSchemaGenerator("string", null, () => "sample text"))();
    }

    public static IAttributeSchemaGenerator GetGenerator(IAttributeInfo attribute)
    {
        return attribute switch
        {
            _ => GetGenerator(attribute.AttributeType)
        };
    }
}
