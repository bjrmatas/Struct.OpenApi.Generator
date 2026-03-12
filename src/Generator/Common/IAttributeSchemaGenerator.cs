using Struct.Api.Models;
using Struct.OpenApi.Generator.Models;

namespace Struct.OpenApi.Generator.Common;

public interface IAttributeSchemaGenerator
{
    OpenApiSchema GenerateSchema(IAttributeInfo attribute, Dictionary<string, OpenApiSchema> complexTypes);
}
