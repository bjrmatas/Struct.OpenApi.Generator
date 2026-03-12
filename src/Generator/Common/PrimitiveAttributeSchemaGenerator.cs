using Struct.Api.Models;
using Struct.OpenApi.Generator.Common.Extensions;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Common;

public sealed class PrimitiveAttributeSchemaGenerator(
    string type,
    string? format) : IAttributeSchemaGenerator
{

    public OpenApiSchema GenerateSchema(
        IAttributeInfo attribute,
        Dictionary<string, OpenApiSchema> complexTypes)
    {
        return attribute.CreateLocalizedOrSegmentedSchema(() => new OpenApiSchema { Type = type, Format = format });
    }
}
