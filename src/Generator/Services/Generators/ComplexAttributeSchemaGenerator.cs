using Struct.Api.Models;
using Struct.OpenApi.Generator.Common;
using Struct.OpenApi.Generator.Common.Extensions;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Services.Generators;

public sealed class ComplexAttributeSchemaGenerator : IAttributeSchemaGenerator
{
    public OpenApiSchema GenerateSchema(
        IAttributeInfo attribute,
        Dictionary<string, OpenApiSchema> complexTypes)
    {
        var info = (ComplexAttributeInfo)attribute;

        if (info.SubAttributes == null)
        {
            return attribute.CreateLocalizedOrSegmentedSchema(() => new OpenApiSchema { Type = "object" });
        }

        var typeName = info.Alias;

        if (!complexTypes.ContainsKey(typeName))
        {
            var complexSchema = new OpenApiSchema { Type = "object", Properties = [] };

            foreach (var subAttr in info.SubAttributes)
            {
                var subAttrInfo = AttributeInfoExtensions.FromAttributeInfo(subAttr);
                var subGenerator = AttributeSchemaGeneratorFactory.GetGenerator(subAttrInfo);
                var subSchema = subGenerator.GenerateSchema(subAttrInfo, complexTypes);
                complexSchema.Properties[subAttr.Alias] = subSchema;
            }

            complexTypes[typeName] = complexSchema;
        }

        return attribute.CreateLocalizedOrSegmentedSchema(() => new OpenApiSchema { Ref = $"#/components/schemas/{typeName}" });
    }

    public object? GenerateDummyValue(
        IAttributeInfo attribute,
        List<Dimension> dimensions,
        List<Language> languages)
    {
        var info = (ComplexAttributeInfo)attribute;

        if (info.SubAttributes == null || info.SubAttributes.Count == 0)
        {
            return new Dictionary<string, object?> { ["key"] = "data" };
        }

        if (attribute.Localized || attribute.HasSegment())
        {
            return attribute.GenerateLocalizedOrSegmentedDummyValue(dimensions, languages, () =>
                GenerateComplexFieldsValue(info.SubAttributes, dimensions, languages));
        }

        return GenerateComplexFieldsValue(info.SubAttributes, dimensions, languages);
    }

    private static Dictionary<string, object?> GenerateComplexFieldsValue(
        List<AttributeInfo> fields,
        List<Dimension> dimensions,
        List<Language> languages)
    {
        var result = new Dictionary<string, object?>();

        foreach (var field in fields)
        {
            var fieldInfo = AttributeInfoExtensions.FromAttributeInfo(field);
            var generator = AttributeSchemaGeneratorFactory.GetGenerator(fieldInfo);
            result[field.Alias] = generator.GenerateDummyValue(fieldInfo, dimensions, languages);
        }

        return result;
    }
}
