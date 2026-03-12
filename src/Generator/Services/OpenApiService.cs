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

        var globalListReferenceProperties = BuildAttributeSchemas(productAttributes, complexTypes, GlobalListSchemaKind.Reference);
        var globalListValueProperties = BuildAttributeSchemas(productAttributes, complexTypes, GlobalListSchemaKind.Value);
        var globalListRefProperties = globalListReferenceProperties.Properties;
        var globalListValProperties = globalListValueProperties.Properties;
        var globalListRefValuesSchema = CreateObjectSchema(globalListRefProperties, globalListReferenceProperties.Required);
        var globalListValValuesSchema = CreateObjectSchema(globalListValProperties, globalListValueProperties.Required);

        var variantGlobalListReferenceProperties = BuildAttributeSchemas(variantAttributes, complexTypes, GlobalListSchemaKind.Reference);
        var variantGlobalListValueProperties = BuildAttributeSchemas(variantAttributes, complexTypes, GlobalListSchemaKind.Value);
        var variantGlobalListRefProperties = variantGlobalListReferenceProperties.Properties;
        var variantGlobalListValProperties = variantGlobalListValueProperties.Properties;
        var variantGlobalListRefValuesSchema = CreateObjectSchema(variantGlobalListRefProperties, variantGlobalListReferenceProperties.Required);
        var variantGlobalListValValuesSchema = CreateObjectSchema(variantGlobalListValProperties, variantGlobalListValueProperties.Required);

        var variantWithGlobalListRefSchema = CreateItemSchema("VariantId", variantGlobalListRefValuesSchema);
        var variantWithGlobalListValSchema = CreateItemSchema("VariantId", variantGlobalListValValuesSchema);

        var productWithGlobalListRefSchema = CreateItemSchema("ProductId", globalListRefValuesSchema);
        var productWithGlobalListValSchema = CreateItemSchema("ProductId", globalListValValuesSchema);

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

    private static (Dictionary<string, OpenApiSchema> Properties, List<string> Required) BuildAttributeSchemas(
        IEnumerable<AttributeInfo> attributes,
        Dictionary<string, OpenApiSchema> complexTypes,
        GlobalListSchemaKind schemaKind)
    {
        var result = new Dictionary<string, OpenApiSchema>();
        var required = new List<string>();

        foreach (var attr in attributes)
        {
            var schema = IsGlobalListAttribute(attr)
                ? ConvertGlobalListSchema(attr, complexTypes, schemaKind)
                : ConvertAttributeSchema(attr, complexTypes);

            if (attr.Mandatory)
            {
                required.Add(attr.Alias);
                schema.Nullable = false;
            }

            result[attr.Alias] = schema;
        }

        return (result, required);
    }

    private static OpenApiSchema ConvertAttributeSchema(AttributeInfo attribute, Dictionary<string, OpenApiSchema> complexTypes)
    {
        var attrInfo = AttributeInfoExtensions.FromAttributeInfo(attribute);
        var generator = AttributeSchemaGeneratorFactory.GetGenerator(attrInfo);
        return generator.GenerateSchema(attrInfo, complexTypes);
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

    private static OpenApiSchema ConvertGlobalListSchema(
        AttributeInfo attribute,
        Dictionary<string, OpenApiSchema> complexTypes,
        GlobalListSchemaKind schemaKind)
    {
        var fixedList = attribute as FixedListAttributeInfo;
        var referencedAttribute = fixedList?.Template ?? fixedList?.ReferencedAttribute;

        var baseSchema = BuildGlobalListBaseSchema(schemaKind, referencedAttribute, complexTypes);
        var wrappedSchema = WrapGlobalListBaseSchema(attribute, baseSchema);

        return attribute.CreateLocalizedOrSegmentedSchema(() => wrappedSchema);
    }

    private static OpenApiSchema BuildGlobalListBaseSchema(
        GlobalListSchemaKind schemaKind,
        AttributeInfo? referencedAttribute,
        Dictionary<string, OpenApiSchema> complexTypes)
    {
        if (schemaKind == GlobalListSchemaKind.Reference)
        {
            return new OpenApiSchema { Type = "string", Format = "uuid" };
        }

        if (referencedAttribute == null)
        {
            return new OpenApiSchema { Type = "string" };
        }

        var typeName = referencedAttribute.Alias;
        EnsureReferencedAttributeSchema(referencedAttribute, complexTypes, typeName);

        return new OpenApiSchema { Ref = $"#/components/schemas/{typeName}" };
    }

    private static OpenApiSchema WrapGlobalListBaseSchema(AttributeInfo attribute, OpenApiSchema baseSchema)
    {
        if (!attribute.AllowMultipleValues)
        {
            return baseSchema;
        }

        return new OpenApiSchema
        {
            Type = "array",
            Items = baseSchema
        };
    }

    private static void EnsureReferencedAttributeSchema(
        AttributeInfo referencedAttribute,
        Dictionary<string, OpenApiSchema> complexTypes,
        string typeName)
    {
        if (complexTypes.ContainsKey(typeName))
        {
            return;
        }

        if (referencedAttribute is ComplexAttributeInfo complexAttribute)
        {
            var complexSchema = new OpenApiSchema { Type = "object", Properties = [] };

            if (complexAttribute.SubAttributes != null)
            {
                foreach (var subAttr in complexAttribute.SubAttributes)
                {
                    var subAttrInfo = AttributeInfoExtensions.FromAttributeInfo(subAttr);
                    var subGenerator = AttributeSchemaGeneratorFactory.GetGenerator(subAttrInfo);
                    var subSchema = subGenerator.GenerateSchema(subAttrInfo, complexTypes);
                    complexSchema.Properties[subAttr.Alias] = subSchema;
                }
            }

            complexTypes[typeName] = complexSchema;
            return;
        }

        var valueAttrInfo = AttributeInfoExtensions.FromAttributeInfo(referencedAttribute);
        var valueGenerator = AttributeSchemaGeneratorFactory.GetGenerator(valueAttrInfo);
        complexTypes[typeName] = valueGenerator.GenerateSchema(valueAttrInfo, complexTypes);
    }

    private enum GlobalListSchemaKind
    {
        Reference,
        Value
    }

}
