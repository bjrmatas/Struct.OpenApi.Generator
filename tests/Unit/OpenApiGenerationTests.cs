using System.Text.Json;
using System.Text.Json.Serialization;
using Struct.Api.Models;
using Struct.OpenApi.Generator.Models;
using Struct.OpenApi.Generator.Services;

namespace Generator.UnitTests;

public sealed class OpenApiGenerationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions SnapshotOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void GenerateSchema_UsesAttributesAndStructures()
    {
        var schemas = GenerateSchemas();

        var components = schemas.Components!.Schemas;
        Assert.True(components.ContainsKey("ProductWithGlobalListReference"));
        Assert.True(components.ContainsKey("ProductWithGlobalListValue"));
        Assert.Contains(components.Keys, k => k is "Age" or "AgeChildren" or "HighlightsConscious" or "TipType");

        var productRefSchema = components["ProductWithGlobalListReference"];
        var productValSchema = components["ProductWithGlobalListValue"];

        Assert.Contains("ProductId", productRefSchema.Properties!.Keys);
        Assert.Contains("ProductId", productValSchema.Properties!.Keys);
        Assert.Contains("Values", productRefSchema.Properties!.Keys);
        Assert.Contains("Values", productValSchema.Properties!.Keys);
        Assert.Contains("Variant", productRefSchema.Properties!.Keys);
        Assert.Contains("Variant", productValSchema.Properties!.Keys);

        var variantRefSchema = components.GetValueOrDefault("VariantWithGlobalListReference");
        var variantValSchema = components.GetValueOrDefault("VariantWithGlobalListValue");

        Assert.NotNull(variantRefSchema);
        Assert.NotNull(variantValSchema);
        Assert.Contains("VariantId", variantRefSchema!.Properties!.Keys);
        Assert.Contains("VariantId", variantValSchema!.Properties!.Keys);

        var productValues = productRefSchema.Properties!["Values"].Properties!;
        Assert.Contains("ReadyForWeb", productValues.Keys);
        Assert.DoesNotContain("ProductName", productValues.Keys);

        var variantValues = variantRefSchema.Properties!["Values"].Properties!;
        Assert.Contains("Age", variantValues.Keys);
        Assert.Contains("AgeChilden", variantValues.Keys);
        Assert.Contains("HighlightsConscious", variantValues.Keys);
        Assert.Contains("HighlightsConsciousCustom", variantValues.Keys);
    }

    [Fact]
    public void GenerateSchema_CreatesComplexTypesForFixedListAttributes()
    {
        var schemas = GenerateSchemas();

        var components = schemas.Components!.Schemas;

        Assert.Contains("Age", components.Keys);
        Assert.Contains("AgeChildren", components.Keys);
        Assert.Contains("HighlightsConscious", components.Keys);
        Assert.Contains("TipType", components.Keys);

        var ageSchema = components["Age"];
        Assert.Equal("object", ageSchema.Type);
        Assert.Contains("Key", ageSchema.Properties!.Keys);
        Assert.Contains("Value", ageSchema.Properties!.Keys);

        var valueSchema = ageSchema.Properties!["Value"];
        Assert.Equal("array", valueSchema.Type);
        Assert.NotNull(valueSchema.Items);
        Assert.Contains("CultureCode", valueSchema.Items!.Properties!.Keys);
        Assert.Contains("Data", valueSchema.Items!.Properties!.Keys);
        Assert.DoesNotContain("Segment", valueSchema.Items!.Properties!.Keys);
    }

    [Fact]
    public void GenerateSchema_UsesMandatoryAttributesForRequiredAndNullable()
    {
        var schemas = GenerateSchemas();
        var components = schemas.Components!.Schemas;

        var productSchema = components["ProductWithGlobalListReference"];
        var productValues = productSchema.Properties!["Values"];

        Assert.Contains("ReadyForWeb", productValues.Required ?? []);
        Assert.False(productValues.Properties!["ReadyForWeb"].Nullable ?? false);

        var variantSchema = components["VariantWithGlobalListReference"];
        var variantValues = variantSchema.Properties!["Values"];

        Assert.Contains("Age", variantValues.Required ?? []);
        Assert.False(variantValues.Properties!["Age"].Nullable ?? false);
    }

    [Fact]
    public void GenerateSchema_MatchesSnapshot()
    {
        var openApiDoc = GenerateSchemas();
        var json = JsonSerializer.Serialize(openApiDoc, SnapshotOptions);
        var snapshotPath = GetSnapshotPath();

        if (ShouldUpdateSnapshots() || !File.Exists(snapshotPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
            File.WriteAllText(snapshotPath, json);
            return;
        }

        var expected = File.ReadAllText(snapshotPath);
        Assert.Equal(expected, json);
    }

    private static List<AttributeInfo> LoadAttributes()
    {
        var json = File.ReadAllText(GetFixturePath("attributes.json"));
        return JsonSerializer.Deserialize<List<AttributeInfo>>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to load attributes.json");
    }

    private static ProductStructure LoadProductStructure()
    {
        var json = File.ReadAllText(GetFixturePath("product-structures.json"));
        var structures = JsonSerializer.Deserialize<List<ProductStructure>>(json, JsonOptions)
            ?? throw new InvalidOperationException("Failed to load product-structures.json");
        return structures[0];
    }

    private static OpenApiDocument GenerateSchemas()
    {
        var attributes = LoadAttributes();
        var structure = LoadProductStructure();
        var (productAttributeUids, variantAttributeUids) = structure.ExtractAttributeUids();

        var productAttributes = attributes
            .Where(a => productAttributeUids.Contains(a.Uid))
            .ToList();
        var variantAttributes = attributes
            .Where(a => variantAttributeUids.Contains(a.Uid))
            .ToList();

        return OpenApiService.CreateOpenApiDocument(productAttributes, variantAttributes);
    }

    private static string GetFixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, fileName);
    }

    private static string GetSnapshotPath()
    {
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        return Path.Combine(projectDir, "expected-openapi.json");
    }

    private static bool ShouldUpdateSnapshots()
    {
        return string.Equals(Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS"), "1", StringComparison.OrdinalIgnoreCase);
    }
}
