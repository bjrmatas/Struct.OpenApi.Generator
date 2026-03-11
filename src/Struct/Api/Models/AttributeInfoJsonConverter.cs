using System.Text.Json;
using System.Text.Json.Serialization;

namespace Struct.Api.Models;

public sealed class AttributeInfoJsonConverter : JsonConverter<AttributeInfo>
{
    public override AttributeInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var raw = document.RootElement.GetRawText();
        var detection = JsonSerializer.Deserialize<AttributeInfoDetection>(raw, options)
            ?? throw new JsonException("Failed to detect attribute type");
        return detection.GetAttributeInfo(raw, options);
    }

    public override void Write(Utf8JsonWriter writer, AttributeInfo value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
    }
}
