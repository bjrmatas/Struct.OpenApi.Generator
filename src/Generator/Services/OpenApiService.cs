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
    /// <returns>A tuple of two lists: product attribute UIDs and variant attribute UIDs.</returns>
    public static (List<string> ProductAttributes, List<string> VariantAttributes) ExtractAttributeUids(ProductStructure structure)
    {
        var productUids = new HashSet<string>();
        var variantUids = new HashSet<string>();

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
                            productUids.Add(prop.AttributeUid);
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
                            variantUids.Add(prop.AttributeUid);
                        }
                    }
                }
            }
        }

        return ([.. productUids], [.. variantUids]);
    }

    /// <summary>
    /// Generates an OpenAPI schema from product and variant attributes wrapped in a default Product object.
    /// </summary>
    /// <param name="productAttributes">The list of product attributes.</param>
    /// <param name="variantAttributes">The list of variant attributes.</param>
    /// <returns>An OpenApiSchema representing a Product object with Values, ProductId, and Variant.</returns>
    public static OpenApiSchema GenerateSchema(List<AttributeInfo> productAttributes, List<AttributeInfo> variantAttributes)
    {
        var valueProperties = new Dictionary<string, OpenApiSchema>();

        foreach (var attr in productAttributes)
        {
            var attrSchema = ConvertAttributeToSchema(attr);
            valueProperties[attr.Alias] = attrSchema;
        }

        var valuesSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = valueProperties
        };

        var variantValueProperties = new Dictionary<string, OpenApiSchema>();

        foreach (var attr in variantAttributes)
        {
            var attrSchema = ConvertAttributeToSchema(attr);
            variantValueProperties[attr.Alias] = attrSchema;
        }

        var variantValuesSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = variantValueProperties
        };

        var variantSchema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["VariantId"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                ["Values"] = variantValuesSchema
            }
        };

        var productProperties = new Dictionary<string, OpenApiSchema>
        {
            ["ProductId"] = new OpenApiSchema { Type = "integer", Format = "int32" },
            ["Values"] = valuesSchema,
            ["Variant"] = variantSchema
        };

        return new OpenApiSchema
        {
            Type = "object",
            Properties = productProperties
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
