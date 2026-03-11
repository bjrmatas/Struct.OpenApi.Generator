using Struct.Api.Models;
using Struct.OpenApi.Generator.Common.Extensions;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Common;

public sealed class PrimitiveAttributeSchemaGenerator(
    string type,
    string? format,
    Func<object?> dummyValueFactory) : IAttributeSchemaGenerator
{

    public OpenApiSchema GenerateSchema(
        IAttributeInfo attribute,
        Dictionary<string, OpenApiSchema> complexTypes)
    {
        return attribute.CreateLocalizedOrSegmentedSchema(() => new OpenApiSchema { Type = type, Format = format });
    }

    public object? GenerateDummyValue(IAttributeInfo attribute, List<Dimension> dimensions, List<Language> languages)
    {
        if (attribute.Localized || attribute.HasSegment())
        {
            return attribute.GenerateLocalizedOrSegmentedDummyValue(dimensions, languages, dummyValueFactory);
        }

        return dummyValueFactory();
    }
}
