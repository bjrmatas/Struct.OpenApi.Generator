using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;
using Struct.OpenApi.Generator.Commands;

Console.OutputEncoding = Encoding.UTF8;

AnsiConsole.Clear();

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<GenerateCommand>("generate");
});

await app.RunAsync(args);
