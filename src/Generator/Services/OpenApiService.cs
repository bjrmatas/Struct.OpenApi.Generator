using Struct.Api.Models;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Services;

public static class OpenApiService
{
    /// <summary>
    /// Creates a new OpenAPI document with default settings.
    /// </summary>
    /// <returns>A new OpenApiDocument instance.</returns>
    public static OpenApiDocument CreateOpenApiDocument()
    {
        return new OpenApiDocument
        {
            OpenApi = "3.0.0",
            Info = new OpenApiInfo
            {
                Title = "Struct API",
                Version = "1.0.0"
            },
            Paths = []
        };
    }

    /// <summary>
    /// Extracts all unique attribute UIDs from a product structure's tabs and sections.
    /// </summary>
    /// <param name="structure">The product structure to extract attribute UIDs from.</param>
    /// <returns>A list of unique attribute UIDs.</returns>
    public static List<string> ExtractAttributeUids(ProductStructure structure)
    {
        var uids = new HashSet<string>();

        if (structure.ProductConfiguration?.Tabs != null)
        {
            foreach (var tab in structure.ProductConfiguration.Tabs)
            {
                if (tab.Sections == null) continue;
                foreach (var section in tab.Sections)
                {
                    if (section.Properties == null) continue;
                    foreach (var prop in section.Properties)
                    {
                        if (!string.IsNullOrEmpty(prop.AttributeUid))
                        {
                            uids.Add(prop.AttributeUid);
                        }
                    }
                }
            }
        }

        if (structure.VariantConfiguration?.Tabs != null)
        {
            foreach (var tab in structure.VariantConfiguration.Tabs)
            {
                if (tab.Sections == null) continue;
                foreach (var section in tab.Sections)
                {
                    if (section.Properties == null) continue;
                    foreach (var prop in section.Properties)
                    {
                        if (!string.IsNullOrEmpty(prop.AttributeUid))
                        {
                            uids.Add(prop.AttributeUid);
                        }
                    }
                }
            }
        }

        return [.. uids];
    }

    /// <summary>
    /// Generates an OpenAPI schema from a list of attributes.
    /// </summary>
    /// <param name="attributes">The list of attributes to generate the schema from.</param>
    /// <returns>An OpenApiSchema representing the attributes as object properties.</returns>
    public static OpenApiSchema GenerateSchema(List<AttributeInfo> attributes)
    {
        var properties = new Dictionary<string, OpenApiSchema>();

        foreach (var attr in attributes)
        {
            var attrSchema = ConvertAttributeToSchema(attr);
            properties[attr.Alias] = attrSchema;
        }

        return new OpenApiSchema
        {
            Type = "object",
            Properties = properties
        };
    }

    /// <summary>
    /// Converts an attribute to its OpenAPI schema representation.
    /// </summary>
    /// <param name="attribute">The attribute to convert.</param>
    /// <returns>An OpenApiSchema representing the attribute.</returns>
    private static OpenApiSchema ConvertAttributeToSchema(AttributeInfo attribute)
    {
        var description = attribute.BackofficeName ?? attribute.Name?.Values.FirstOrDefault();
        var schema = new OpenApiSchema
        {
            Description = description
        };

        var hasSegment = !string.IsNullOrEmpty(attribute.DimensionUid);

        if (attribute.Localized || hasSegment)
        {
            schema.Type = "array";
            schema.Items = new OpenApiSchema
            {
                Type = "object",
                Properties = []
            };

            if (hasSegment)
            {
                schema.Items.Properties!["Segment"] = new OpenApiSchema { Type = "string" };
            }

            if (attribute.Localized)
            {
                schema.Items.Properties!["CultureCode"] = new OpenApiSchema { Type = "string" };
            }

            var valueSchema = GetBaseTypeSchema(attribute);
            schema.Items.Properties!["Value"] = valueSchema;
        }
        else
        {
            var baseSchema = GetBaseTypeSchema(attribute);
            schema.Type = baseSchema.Type;
            schema.Format = baseSchema.Format;
            schema.Items = baseSchema.Items;
        }

        return schema;
    }

    /// <summary>
    /// Gets the base OpenAPI schema type for an attribute based on its type.
    /// </summary>
    /// <param name="attribute">The attribute to get the base type schema for.</param>
    /// <returns>An OpenApiSchema with the appropriate type and format.</returns>
    private static OpenApiSchema GetBaseTypeSchema(AttributeInfo attribute)
    {
        var schema = new OpenApiSchema();
        var attrType = attribute.AttributeType?.ToLowerInvariant() ?? "";

        if (attrType.Contains("complex"))
        {
            schema.Type = "object";
        }
        else
        {
            schema.Type = attrType switch
            {
                "numberattribute" or "decimalattribute" => "number",
                "booleanattribute" => "boolean",
                "dateattribute" => "string",
                "datetimeattribute" => "string",
                "assetreferenceattribute" or "imagereferenceattribute" => "string",
                _ => "string"
            };
        }

        return schema;
    }
}
