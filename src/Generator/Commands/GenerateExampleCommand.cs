using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using Struct.Api;
using Struct.Api.Models;
using Struct.OpenApi.Generator.Common;
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

    public enum SchemaKind
    {
        Reference,
        Value
    }

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[TYPE]")]
        [Description("Type of example: product or variant")]
        public ItemType? Type { get; set; }

        [CommandArgument(1, "[SCHEMA]")]
        [Description("Schema kind: reference or value")]
        public SchemaKind? Schema { get; set; }

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

        var itemType = settings.Type ?? PromptForItemType();
        var schemaKind = settings.Schema ?? PromptForSchemaKind();

        var attributeUids = itemType == ItemType.Product ? productAttributeUids : variantAttributeUids;
        console.MarkupLine($"  Found [cyan]{attributeUids.Count}[/] {itemType.ToString().ToLower()} attributes");

        var attributes = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync("Loading attributes...", async _ => await client.GetAttributesBatchAsync(attributeUids));

        var otherAttributeUids = itemType == ItemType.Product ? variantAttributeUids : productAttributeUids;
        var otherAttributes = await AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Star)
            .StartAsync("Loading other attributes...", async _ => await client.GetAttributesBatchAsync(otherAttributeUids));

        console.MarkupLine($"  [green]Successfully loaded {attributes.Count} attributes[/]");

        var openApiDoc = itemType == ItemType.Product
            ? OpenApiService.CreateOpenApiDocument(attributes, otherAttributes)
            : OpenApiService.CreateOpenApiDocument(otherAttributes, attributes);

        var schemaName = GetSchemaName(itemType, schemaKind);
        if (openApiDoc.Components?.Schemas == null ||
            !openApiDoc.Components.Schemas.TryGetValue(schemaName, out var targetSchema))
        {
            console.MarkupLine($"[red]Schema '{schemaName}' not found.[/]");
            return 1;
        }

        var example = AttributeSchemaGeneratorFactory.GenerateExample(targetSchema, openApiDoc);

        var outputPath = settings.OutputPath ?? $"example-{itemType.ToString().ToLower()}-{schemaKind.ToString().ToLower()}.json";
        var json = JsonSerializer.Serialize(example, JsonSerializerOptions);

        await File.WriteAllTextAsync(outputPath, json, cancellationToken);

        console.WriteLine();
        console.MarkupLine($"[bold green]Example generation complete![/]");
        console.MarkupLine($"[cyan]Output written to:[/] {outputPath}");

        return 0;
    }

    private static TEnum PromptForEnum<TEnum>(string title) where TEnum : struct, Enum
    {
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(title)
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to select)[/]")
                .AddChoices(Enum.GetValues<TEnum>().Select(e => e.ToString())));

        return Enum.Parse<TEnum>(selection);
    }

    private static ItemType PromptForItemType() => PromptForEnum<ItemType>("Select item type:");

    private static SchemaKind PromptForSchemaKind() => PromptForEnum<SchemaKind>("Select schema kind:");

    private static string GetSchemaName(ItemType type, SchemaKind schema)
    {
        var typeName = type == ItemType.Product ? "Product" : "Variant";
        var schemaName = schema == SchemaKind.Reference ? "Reference" : "Value";
        return $"{typeName}WithGlobalList{schemaName}";
    }
}
