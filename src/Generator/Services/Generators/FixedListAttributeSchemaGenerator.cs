using Struct.Api.Models;
using Struct.OpenApi.Generator.Common;
using Struct.OpenApi.Generator.Common.Extensions;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Services.Generators;

public sealed class FixedListAttributeSchemaGenerator : IAttributeSchemaGenerator
{
    public OpenApiSchema GenerateSchema(IAttributeInfo attribute, Dictionary<string, OpenApiSchema> complexTypes)
    {
        return attribute.CreateLocalizedOrSegmentedSchema(() => new OpenApiSchema { Type = "string" });
    }

    public object? GenerateDummyValue(IAttributeInfo attribute, List<Dimension> dimensions, List<Language> languages)
    {
        if (attribute.Localized || attribute.HasSegment())
        {
            return attribute.GenerateLocalizedOrSegmentedDummyValue(dimensions, languages, () => "dummy-value");
        }

        return "dummy-value";
    }
}
