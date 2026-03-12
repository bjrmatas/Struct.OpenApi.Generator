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
}
