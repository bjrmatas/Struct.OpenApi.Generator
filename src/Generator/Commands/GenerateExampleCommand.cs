using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Struct.Api;
using Struct.Api.Models;
using Struct.OpenApi.Generator.Services;

namespace Struct.OpenApi.Generator.Commands;

public class GenerateExampleCommand : AsyncCommand<GenerateExampleCommand.Settings>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public enum ItemType
    {
        Product,
        Variant
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<TYPE>")]
        [Description("Type of example: product or variant")]
        public ItemType Type { get; set; }

        [CommandOption("--output")]
        public string? OutputPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var console = AnsiConsole.Console;

        console.Write(new Rule("[bold blue]Struct OpenAPI Example Generator[/]").RuleStyle("blue"));

        var options = ConfigurationService.GetClientOptions();
        var client = StructClient.Create(options);

        console.WriteLine();
        console.MarkupLine("[bold]Fetching product structures...[/]");

        var structures = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync("Loading product structures...", async ctx => await client.GetProductStructuresAsync());

        if (structures.Count == 0)
        {
            console.MarkupLine("[red]No product structures found.[/]");
            return 1;
        }

        console.MarkupLine($"[green]Found {structures.Count} product structure(s)[/]");
        console.WriteLine();

        var selectedStructure = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a product structure:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to select)[/]")
                .AddChoices(structures.Select(s => $"{s.Alias} ({s.Label})")));

        var structure = structures.First(s => $"{s.Alias} ({s.Label})" == selectedStructure);

        console.WriteLine();
        console.MarkupLine($"[bold]Processing:[/] {structure.Alias}");

        var fullStructure = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync($"Loading {structure.Alias}...", async ctx => await client.GetProductStructureAsync(structure.Uid));

        var (productAttributeUids, variantAttributeUids) = OpenApiService.ExtractAttributeUids(fullStructure);

        var attributeUids = settings.Type == ItemType.Product ? productAttributeUids : variantAttributeUids;
        console.MarkupLine($"  Found [cyan]{attributeUids.Count}[/] {settings.Type.ToString().ToLower()} attributes");

        var attributes = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync("Loading attributes...", async _ => await client.GetAttributesBatchAsync(attributeUids));

        var dimensions = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync("Loading dimensions...", async _ => await client.GetDimensionsAsync());

        var languages = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync("Loading languages...", async _ => await client.GetLanguagesAsync());

        console.MarkupLine($"  [green]Successfully loaded {attributes.Count} attributes[/]");
        console.MarkupLine($"  [green]Successfully loaded {dimensions.Count} dimensions[/]");
        console.MarkupLine($"  [green]Successfully loaded {languages.Count} languages[/]");

        var example = GenerateExampleData(attributes, settings.Type, dimensions, languages);

        var outputPath = settings.OutputPath ?? $"example-{settings.Type.ToString().ToLower()}.json";
        var json = JsonSerializer.Serialize(example, JsonSerializerOptions);

        await File.WriteAllTextAsync(outputPath, json, cancellationToken);

        console.WriteLine();
        console.MarkupLine($"[bold green]Example generation complete![/]");
        console.MarkupLine($"[cyan]Output written to:[/] {outputPath}");

        return 0;
    }

    private static Dictionary<string, object?> GenerateExampleData(List<AttributeInfo> attributes, ItemType type, List<Dimension> dimensions, List<Language> languages)
    {
        var result = new Dictionary<string, object?>();

        if (type == ItemType.Product)
        {
            result["ProductId"] = 1;
        }
        else if (type == ItemType.Variant)
        {
            result["VariantId"] = 1;
        }

        var values = new Dictionary<string, object?>();

        foreach (var attr in attributes)
        {
            values[attr.Alias] = GenerateDummyValue(attr, dimensions, languages);
        }

        result["Values"] = values;

        return result;
    }

    private static object? GenerateDummyValue(AttributeInfo attribute, List<Dimension> dimensions, List<Language> languages)
    {
        var attrType = attribute.AttributeType?.ToLowerInvariant() ?? "";

        if (attribute.Localized || !string.IsNullOrEmpty(attribute.DimensionUid))
        {
            var dimension = !string.IsNullOrEmpty(attribute.DimensionUid)
                ? dimensions.FirstOrDefault(d => d.Uid == attribute.DimensionUid)
                : null;

            var segments = dimension?.Segments ?? [];
            var langs = attribute.Localized ? languages : [new Language { CultureCode = "", Name = "" }];

            if (segments.Count == 0)
            {
                segments = [new DimensionSegment { Uid = "", Identifier = "default", Name = "" }];
            }

            var items = new List<Dictionary<string, object?>>();

            foreach (var segment in segments)
            {
                foreach (var lang in langs)
                {
                    var item = new Dictionary<string, object?>();

                    if (!string.IsNullOrEmpty(attribute.DimensionUid))
                    {
                        item["Segment"] = segment.Identifier;
                    }

                    if (attribute.Localized)
                    {
                        item["CultureCode"] = lang.CultureCode;
                    }

                    item["Value"] = GetBaseTypeDummyValue(attrType);
                    items.Add(item);
                }
            }

            return items;
        }

        return GetBaseTypeDummyValue(attrType);
    }

    private static object? GetBaseTypeDummyValue(string attrType)
    {
        return attrType switch
        {
            "numberattribute" or "decimalattribute" => 42.5,
            "booleanattribute" => true,
            "dateattribute" => "2024-01-15",
            "datetimeattribute" => "2024-01-15T10:30:00Z",
            "assetreferenceattribute" or "imagereferenceattribute" => "/assets/example.jpg",
            "complexattribute" => new Dictionary<string, object?> { ["key"] = "value" },
            _ => "sample text"
        };
    }
}
