using Struct.Api.Models;
using Struct.OpenApi.Generator.Common;
using Struct.OpenApi.Generator.Common.Extensions;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Services;

public static class OpenApiService
{
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
    /// Generates an OpenAPI document from product and variant attributes.
    /// </summary>
    /// <param name="productAttributes">The list of product attributes.</param>
    /// <param name="variantAttributes">The list of variant attributes.</param>
    /// <returns>An OpenApiDocument with schemas for products, variants, and complex types.</returns>
    public static OpenApiDocument CreateOpenApiDocument(List<AttributeInfo> productAttributes, List<AttributeInfo> variantAttributes)
    {
        var complexTypes = new Dictionary<string, OpenApiSchema>();

        var globalListProperties = BuildGlobalListSchemas(productAttributes, complexTypes);
        var globalListRefProperties = globalListProperties.Properties;
        var globalListValProperties = globalListProperties.Properties;
        var globalListRefValuesSchema = CreateObjectSchema(globalListRefProperties, globalListProperties.Required);
        var globalListValValuesSchema = CreateObjectSchema(globalListValProperties, globalListProperties.Required);

        var variantGlobalListProperties = BuildGlobalListSchemas(variantAttributes, complexTypes);
        var variantGlobalListRefProperties = variantGlobalListProperties.Properties;
        var variantGlobalListValProperties = variantGlobalListProperties.Properties;
        var variantGlobalListRefValuesSchema = CreateObjectSchema(variantGlobalListRefProperties, variantGlobalListProperties.Required);
        var variantGlobalListValValuesSchema = CreateObjectSchema(variantGlobalListValProperties, variantGlobalListProperties.Required);

        var variantWithGlobalListRefSchema = CreateItemSchema("VariantId", variantGlobalListRefValuesSchema);
        var variantWithGlobalListValSchema = CreateItemSchema("VariantId", variantGlobalListValValuesSchema);

        var productWithGlobalListRefSchema = CreateItemSchema("ProductId", globalListRefValuesSchema, variantWithGlobalListRefSchema);
        var productWithGlobalListValSchema = CreateItemSchema("ProductId", globalListValValuesSchema, variantWithGlobalListValSchema);

        var openApiDoc = CreateOpenApiDocument();
        openApiDoc.Components ??= new OpenApiComponents();

        openApiDoc.Components.Schemas["ProductWithGlobalListReference"] = productWithGlobalListRefSchema;
        openApiDoc.Components.Schemas["ProductWithGlobalListValue"] = productWithGlobalListValSchema;

        if (variantGlobalListRefProperties.Count > 0)
        {
            openApiDoc.Components.Schemas["VariantWithGlobalListReference"] = variantWithGlobalListRefSchema;
        }

        if (variantGlobalListValProperties.Count > 0)
        {
            openApiDoc.Components.Schemas["VariantWithGlobalListValue"] = variantWithGlobalListValSchema;
        }

        foreach (var complexType in complexTypes)
        {
            openApiDoc.Components.Schemas[complexType.Key] = complexType.Value;
        }

        return openApiDoc;
    }

    private static (Dictionary<string, OpenApiSchema> Properties, List<string> Required) BuildGlobalListSchemas(
        IEnumerable<AttributeInfo> attributes,
        Dictionary<string, OpenApiSchema> complexTypes)
    {
        var result = new Dictionary<string, OpenApiSchema>();
        var required = new List<string>();

        foreach (var attr in attributes)
        {
            if (!IsGlobalListAttribute(attr))
            {
                continue;
            }

            var schema = ConvertGlobalListSchema(attr, complexTypes);

            if (attr.Mandatory)
            {
                required.Add(attr.Alias);
                schema.Nullable = false;
            }

            result[attr.Alias] = schema;
        }

        return (result, required);
    }

    private static OpenApiSchema CreateObjectSchema(Dictionary<string, OpenApiSchema> properties, List<string>? required = null)
    {
        return new OpenApiSchema
        {
            Type = "object",
            Properties = properties,
            Required = required != null && required.Count > 0 ? required : null
        };
    }

    private static OpenApiSchema CreateItemSchema(string idPropertyName, OpenApiSchema valuesSchema, OpenApiSchema? variantSchema = null)
    {
        var properties = new Dictionary<string, OpenApiSchema>
        {
            [idPropertyName] = new OpenApiSchema { Type = "integer", Format = "int32" },
            ["Values"] = valuesSchema
        };

        if (variantSchema != null)
        {
            properties["Variant"] = variantSchema;
        }

        return CreateObjectSchema(properties);
    }

    private static bool IsGlobalListAttribute(AttributeInfo attribute)
    {
        return !string.IsNullOrEmpty(attribute.GlobalListUid);
    }

    private static OpenApiSchema ConvertGlobalListSchema(AttributeInfo attribute, Dictionary<string, OpenApiSchema> complexTypes)
    {
        var fixedList = attribute as FixedListAttributeInfo;
        var referencedAttribute = fixedList?.Template ?? fixedList?.ReferencedAttribute;

        OpenApiSchema valueSchema;

        if (referencedAttribute != null)
        {
            var valueAttrInfo = AttributeInfoExtensions.FromAttributeInfo(referencedAttribute);
            var valueGenerator = AttributeSchemaGeneratorFactory.GetGenerator(valueAttrInfo);
            valueSchema = valueGenerator.GenerateSchema(valueAttrInfo, complexTypes);
        }
        else
        {
            valueSchema = new OpenApiSchema { Type = "string" };
        }

        return attribute.CreateLocalizedOrSegmentedSchema(() => valueSchema);
    }

}
