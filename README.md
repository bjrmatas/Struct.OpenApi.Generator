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
    ├── Commands/GenerateCommand.cs   # CLI generate command
    ├── Services/ConfigurationService.cs
    └── Struct.OpenApi.Generator.csproj
```

## Configuration

Create `appsettings.json` in the Generator project directory:

```json
{
    "Url": "https://your-struct-api-url.com/",
    "ApiKey": "your-api-key-here"
}
```

## Running

```bash
cd src/Generator
dotnet run -- generate
```

## Options

- `--output <path>` - Output file path (default: `openapi-{timestamp}.json`)

## Example Output

The generator produces an OpenAPI 3.0 document with schemas like:

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
      }
    }
  }
}
```
