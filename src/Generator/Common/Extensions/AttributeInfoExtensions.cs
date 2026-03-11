using Struct.Api.Models;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Common.Extensions;

public static class AttributeInfoExtensions
{
    public static bool HasSegment(this IAttributeInfo info) => !string.IsNullOrEmpty(info.DimensionUid);

    public static string? GetDescription(this IAttributeInfo info) => info.BackofficeName ?? info.Name?.Values.FirstOrDefault();

    public static OpenApiSchema CreateLocalizedOrSegmentedSchema(
        this IAttributeInfo attribute,
        Func<OpenApiSchema> baseSchemaFactory)
    {
        var schema = new OpenApiSchema
        {
            Description = attribute.GetDescription()
        };

        if (attribute.Localized || attribute.HasSegment())
        {
            schema.Type = "array";
            schema.Items = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            if (attribute.HasSegment())
            {
                schema.Items.Properties!["Segment"] = new OpenApiSchema { Type = "string" };
            }

            if (attribute.Localized)
            {
                schema.Items.Properties!["CultureCode"] = new OpenApiSchema { Type = "string" };
            }

            schema.Items.Properties!["Data"] = baseSchemaFactory();
        }
        else
        {
            var baseSchema = baseSchemaFactory();
            schema.Type = baseSchema.Type;
            schema.Format = baseSchema.Format;
            schema.Items = baseSchema.Items;
            schema.Ref = baseSchema.Ref;
        }

        if (!attribute.Mandatory)
        {
            schema.Nullable ??= true;
        }

        return schema;
    }

    public static List<Dictionary<string, object?>> GenerateLocalizedOrSegmentedDummyValue(
        this IAttributeInfo attribute,
        List<Dimension> dimensions,
        List<Language> languages,
        Func<object?> baseValueFactory)
    {
        var dimension = attribute.HasSegment()
            ? dimensions.FirstOrDefault(d => d.Uid == attribute.DimensionUid)
            : null;

        var segments = dimension?.Segments ?? [];
        var langs = attribute.Localized ? languages : [new Language { CultureCode = "", Name = "" }];

        if (segments.Count == 0)
        {
            segments = [new DimensionSegment { Uid = "", Identifier = "default", Name = "" }];
        }

        var items = new List<Dictionary<string, object?>>();

        foreach (var segment in segments)
        {
            foreach (var lang in langs)
            {
                var item = new Dictionary<string, object?>();

                if (attribute.HasSegment())
                {
                    item["Segment"] = segment.Identifier;
                }

                if (attribute.Localized)
                {
                    item["CultureCode"] = lang.CultureCode;
                }

                item["Data"] = baseValueFactory();
                items.Add(item);
            }
        }

        return items;
    }

    public static IAttributeInfo FromAttributeInfo(AttributeInfo info)
    {
        return info;
    }
}
