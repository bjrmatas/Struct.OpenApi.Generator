using Struct.Api.Models;
using Struct.OpenApi.Generator.Services.Generators;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Common;

public static class AttributeSchemaGeneratorFactory
{
    private static readonly Dictionary<string, Func<IAttributeSchemaGenerator>> Generators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["textattribute"] = () => new PrimitiveAttributeSchemaGenerator("string", null),
        ["numberattribute"] = () => new PrimitiveAttributeSchemaGenerator("number", null),
        ["booleanattribute"] = () => new PrimitiveAttributeSchemaGenerator("boolean", null),
        ["datetimeattribute"] = () => new PrimitiveAttributeSchemaGenerator("string", "date-time"),
        ["assetreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32"),
        ["categoryreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32"),
        ["productreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32"),
        ["variantreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32"),
        ["variantgroupreferenceattribute"] = () => new PrimitiveAttributeSchemaGenerator("integer", "int32"),
        ["listattribute"] = () => new ListAttributeSchemaGenerator(),
        ["fixedlistattribute"] = () => new FixedListAttributeSchemaGenerator(),
    };

    public static IAttributeSchemaGenerator GetGenerator(string? attributeType)
    {
        if (string.IsNullOrEmpty(attributeType))
        {
            return new PrimitiveAttributeSchemaGenerator("string", null);
        }

        if (attributeType.Contains("complex", StringComparison.OrdinalIgnoreCase))
        {
            return new ComplexAttributeSchemaGenerator();
        }

        return Generators.GetValueOrDefault(attributeType, () => new PrimitiveAttributeSchemaGenerator("string", null))();
    }

    public static IAttributeSchemaGenerator GetGenerator(IAttributeInfo attribute)
    {
        return attribute switch
        {
            _ => GetGenerator(attribute.AttributeType)
        };
    }

    public static object? GenerateExample(OpenApiSchema schema, OpenApiDocument document)
    {
        return GenerateExample(schema, document, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
    }

    private static object? GenerateExample(OpenApiSchema schema, OpenApiDocument document, HashSet<string> resolving)
    {
        if (!string.IsNullOrEmpty(schema.Ref))
        {
            var refName = ResolveRefName(schema.Ref);
            if (refName == null)
            {
                return null;
            }

            if (!resolving.Add(refName))
            {
                return null;
            }

            var resolvedSchema = ResolveSchema(document, refName);
            var example = resolvedSchema == null ? null : GenerateExample(resolvedSchema, document, resolving);
            resolving.Remove(refName);
            return example;
        }

        if (string.Equals(schema.Type, "object", StringComparison.OrdinalIgnoreCase))
        {
            return GenerateObjectExample(schema, document, resolving);
        }

        if (string.Equals(schema.Type, "array", StringComparison.OrdinalIgnoreCase))
        {
            return GenerateArrayExample(schema, document, resolving);
        }

        return GeneratePrimitiveExample(schema);
    }

    private static object GenerateObjectExample(OpenApiSchema schema, OpenApiDocument document, HashSet<string> resolving)
    {
        var result = new Dictionary<string, object?>();

        if (schema.Properties == null)
        {
            return result;
        }

        foreach (var entry in schema.Properties)
        {
            var value = GenerateExample(entry.Value, document, resolving);
            result[entry.Key] = value;
        }

        return result;
    }

    private static object GenerateArrayExample(OpenApiSchema schema, OpenApiDocument document, HashSet<string> resolving)
    {
        if (schema.Items == null)
        {
            return Array.Empty<object?>();
        }

        return new[] { GenerateExample(schema.Items, document, resolving) };
    }

    private static object? GeneratePrimitiveExample(OpenApiSchema schema)
    {
        if (string.Equals(schema.Type, "string", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(schema.Format, "uuid", StringComparison.OrdinalIgnoreCase))
            {
                return "00000000-0000-0000-0000-000000000000";
            }

            if (string.Equals(schema.Format, "date-time", StringComparison.OrdinalIgnoreCase))
            {
                return "2024-01-15T10:30:00Z";
            }

            return "sample text";
        }

        if (string.Equals(schema.Type, "integer", StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (string.Equals(schema.Type, "number", StringComparison.OrdinalIgnoreCase))
        {
            return 42.5;
        }

        if (string.Equals(schema.Type, "boolean", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return null;
    }

    private static OpenApiSchema? ResolveSchema(OpenApiDocument document, string name)
    {
        if (document.Components?.Schemas == null)
        {
            return null;
        }

        return document.Components.Schemas.GetValueOrDefault(name);
    }

    private static string? ResolveRefName(string? refValue)
    {
        if (string.IsNullOrEmpty(refValue))
        {
            return null;
        }

        const string prefix = "#/components/schemas/";

        if (!refValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return refValue[prefix.Length..];
    }
}
