using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;
using Spectre.Console.Cli;
using Struct.Api;
using Struct.OpenApi.Generator.Models;
using Struct.OpenApi.Generator.Services;

namespace Struct.OpenApi.Generator.Commands;

public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public class Settings : CommandSettings
    {
        [CommandOption("--output")]
        public string? OutputPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var console = AnsiConsole.Console;

        console.Write(new Rule("[bold blue]Struct OpenAPI Generator[/]").RuleStyle("blue"));

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

        var selectedStructures = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select product structures to generate:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to select, press space to toggle)[/]")
                .InstructionsText("[grey](Press Enter to generate)[/]")
                .AddChoices(structures.Select(s => $"{s.Alias} ({s.Label})")));

        console.WriteLine();
        console.MarkupLine($"[green]Selected {selectedStructures.Count} product structure(s)[/]");

        var openApiDoc = OpenApiService.CreateOpenApiDocument();

        foreach (var selected in selectedStructures)
        {
            var structure = structures.First(s => $"{s.Alias} ({s.Label})" == selected);

            console.WriteLine();
            console.MarkupLine($"[bold]Processing:[/] {structure.Alias}");

            var fullStructure = await AnsiConsole
                .Status()
                .Spinner(Spinner.Known.Star)
                .StartAsync($"Loading {structure.Alias}...", async ctx => await client.GetProductStructureAsync(structure.Uid));

            var attributeUids = OpenApiService.ExtractAttributeUids(fullStructure);
            console.MarkupLine($"  Found [cyan]{attributeUids.Count}[/] attributes");

            var attributes = await AnsiConsole
                .Status()
                .Spinner(Spinner.Known.Star)
                .StartAsync("Loading attributes...", async _ => await client.GetAttributesBatchAsync(attributeUids));

            console.MarkupLine($"  [green]Successfully loaded {attributes.Count} attributes[/]");

            var schema = OpenApiService.GenerateSchema(attributes);

            openApiDoc.Components ??= new OpenApiComponents();

            openApiDoc.Components.Schemas[fullStructure.Alias] = schema;
        }

        var outputPath = settings.OutputPath ?? $"openapi-{DateTime.Now:yyyyMMddHHmmss}.json";
        var json = JsonSerializer.Serialize(openApiDoc, JsonSerializerOptions);

        await File.WriteAllTextAsync(outputPath, json, cancellationToken);

        console.WriteLine();
        console.MarkupLine($"[bold green]Generation complete![/]");
        console.MarkupLine($"[cyan]Output written to:[/] {outputPath}");

        return 0;
    }
}
