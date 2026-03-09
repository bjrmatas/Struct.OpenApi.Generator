using System.Text.Json.Serialization;

namespace Struct.OpenApi.Generator.Models;

public class OpenApiDocument
{
    [JsonPropertyName("openapi")]
    public string OpenApi { get; init; } = string.Empty;

    [JsonPropertyName("info")]
    public OpenApiInfo Info { get; init; } = new();

    [JsonPropertyName("paths")]
    public Dictionary<string, OpenApiPathItem> Paths { get; init; } = new();

    [JsonPropertyName("components")]
    public OpenApiComponents? Components { get; set; }
}

public class OpenApiInfo
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}

public class OpenApiPathItem
{
}

public class OpenApiComponents
{
    [JsonPropertyName("schemas")]
    public Dictionary<string, OpenApiSchema> Schemas { get; init; } = new();
}

public class OpenApiSchema
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, OpenApiSchema>? Properties { get; set; }

    [JsonPropertyName("items")]
    public OpenApiSchema? Items { get; set; }
}
