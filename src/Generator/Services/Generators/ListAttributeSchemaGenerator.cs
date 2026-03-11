using Struct.Api.Models;
using Struct.OpenApi.Generator.Common;
using Struct.OpenApi.Generator.Common.Extensions;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Services.Generators;

public sealed class ListAttributeSchemaGenerator : IAttributeSchemaGenerator
{
    public OpenApiSchema GenerateSchema(IAttributeInfo attribute, Dictionary<string, OpenApiSchema> complexTypes)
    {
        var description = attribute.GetDescription();

        return new OpenApiSchema
        {
            Description = description,
            Type = "array",
            Items = new OpenApiSchema { Type = "string" }
        };
    }

    public object? GenerateDummyValue(IAttributeInfo attribute, List<Dimension> dimensions, List<Language> languages)
    {
        return new[] { "item1", "item2" };
    }
}
