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
}
