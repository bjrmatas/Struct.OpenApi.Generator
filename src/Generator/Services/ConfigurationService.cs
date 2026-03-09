using Microsoft.Extensions.Configuration;
using Struct.Api.Common;

namespace Struct.OpenApi.Generator.Services;

public static class ConfigurationService
{
    public static ClientOptions GetClientOptions()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var url = configuration["Url"] ?? throw new InvalidOperationException("Url is required in appsettings.json");
        var apiKey = configuration["ApiKey"] ?? throw new InvalidOperationException("ApiKey is required in appsettings.json");

        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("ApiKey cannot be empty. Please add your API key to appsettings.json");

        return new ClientOptions
        {
            Url = url,
            ApiKey = apiKey
        };
    }
}
