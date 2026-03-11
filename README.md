# Struct OpenAPI Generator

A CLI tool that generates OpenAPI schemas from Struct product structures.

## What It Does

1. Fetches available product structures from Struct API (`/v1/productstructures`)
2. Allows selection of product structures via interactive CLI prompt
3. Fetches selected product structure details (`/v1/productstructures/{uid}`)
4. Extracts and fetches all attributes (`/v1/attributes/{uid}`)
5. Generates OpenAPI 3.0 schemas with support for:
   - Simple types (string, integer, number, boolean)
   - Localized attributes (array with `CultureCode` and `Value`)
   - Segmented attributes (array with `Segment` and `Value`)
   - Complex types

## Project Structure

```
src/
├── Struct/
│   ├── Api/
│   │   ├── Common/ClientOptions.cs    # Configuration options
│   │   ├── Models/                   # API response models
│   │   └── Client.cs                 # HTTP client for Struct API
│   └── Struct.csproj
└── Generator/
    ├── Commands/
    │   ├── GenerateCommand.cs        # CLI generate command
    │   └── GenerateExampleCommand.cs # CLI example command
    ├── Services/ConfigurationService.cs
    └── Struct.OpenApiGenerator.csproj
```

## Configuration

Create `appsettings.json` in `src/Generator`:

```json
{
    "Url": "https://your-struct-api-url.com/",
    "ApiKey": "your-api-key-here"
}
```

`appsettings.json` is copied to the output directory during build and publish, and configuration is loaded using `AppContext.BaseDirectory`. This means the tool can be launched from any working directory.

## Running

```bash
dotnet run --project src/Generator/Struct.OpenApi.Generator.csproj -- <command>
```

You can still run from inside `src/Generator` if preferred.

## Publish

```bash
dotnet publish Struct.OpenApi.Generator.sln
```

The published output includes `appsettings.json`, so the executable has the required configuration file next to it.

## Commands

### generate

Generates OpenAPI schemas from product structures.

```bash
dotnet run -- generate
```

Options:
- `--output <path>` - Output file path (default: `openapi.json`)

### example

Generates example JSON with dummy data for a product or variant.

```bash
dotnet run -- example product
dotnet run -- example variant
```

Options:
- `--output <path>` - Output file path (default: `example-{type}.json`)

The example command fetches dimensions and languages from the API and generates dummy data for all segments.

## Schema Structure

The `generate` command produces an OpenAPI 3.0 document where each schema is wrapped in a Product object:

```json
{
  "openapi": "3.0.0",
  "info": {
    "title": "Struct API",
    "version": "1.0.0"
  },
  "components": {
    "schemas": {
      "NonFood": {
        "type": "object",
        "properties": {
          "ProductId": {
            "type": "integer",
            "format": "int32"
          },
          "Values": {
            "type": "object",
            "properties": {
              "ProductName": {
                "type": "array",
                "items": {
                  "type": "object",
                  "properties": {
                    "CultureCode": { "type": "string" },
                    "Value": { "type": "string" }
                  }
                }
              },
              "Price": {
                "type": "number"
              }
            }
          },
          "Variant": {
            "type": "object",
            "properties": {
              "Values": {
                "type": "object",
                "properties": {
                  "VariantName": { "type": "string" }
                }
              }
            }
          }
        }
      }
    }
  }
}
```

## Example Output

The `example` command produces JSON with dummy data:

```json
{
  "ProductId": 1,
  "Values": {
    "ProductName": [
      {
        "CultureCode": "en-US",
        "Value": "Sample text"
      }
    ],
    "Price": 42.5,
    "StoreData": [
      {
        "Segment": "store1",
        "Value": "Sample text"
      },
      {
        "Segment": "kicks",
        "Value": "Sample text"
      }
    ]
  }
}
```
