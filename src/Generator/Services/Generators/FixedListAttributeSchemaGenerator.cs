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
}
