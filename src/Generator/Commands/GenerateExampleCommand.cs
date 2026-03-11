using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Struct.Api;
using Struct.Api.Models;
using Struct.OpenApi.Generator.Common;
using Struct.OpenApi.Generator.Common.Extensions;
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

        var (productAttributeUids, variantAttributeUids) = fullStructure.ExtractAttributeUids();

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
            var attrInfo = AttributeInfoExtensions.FromAttributeInfo(attr);
            var generator = AttributeSchemaGeneratorFactory.GetGenerator(attrInfo);
            values[attr.Alias] = generator.GenerateDummyValue(attrInfo, dimensions, languages);
        }

        result["Values"] = values;

        return result;
    }
}
