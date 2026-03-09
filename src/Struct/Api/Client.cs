using System.Text.Json;
using Struct.Api.Common;
using Struct.Api.Models;

namespace Struct.Api;

public sealed class StructClient(HttpClient httpClient, string baseUrl)
{
    public async Task<List<ProductStructureSummary>> GetProductStructuresAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/v1/productstructures", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<ProductStructureSummary>>(content) ?? [];
    }

    public async Task<ProductStructure> GetProductStructureAsync(string uid, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/v1/productstructures/{uid}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProductStructure>(content)
            ?? throw new InvalidOperationException("Failed to deserialize product structure");
    }

    public async Task<AttributeInfo> GetAttributeAsync(string uid, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/v1/attributes/{uid}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<AttributeInfo>(content)
            ?? throw new InvalidOperationException("Failed to deserialize attribute");
    }

    public async Task<List<AttributeInfo>> GetAttributesBatchAsync(IEnumerable<string> uids, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(uids.ToArray());
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{baseUrl}/v1/attributes/batch", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<AttributeInfo>>(responseContent) ?? [];
    }

    public async Task<List<Dimension>> GetDimensionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/v1/dimensions", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Dimension>>(content) ?? [];
    }

    public async Task<List<Language>> GetLanguagesAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"{baseUrl}/v1/languages", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Language>>(content) ?? [];
    }

    public static StructClient Create(ClientOptions options)
    {
        var baseUrl = options.Url.TrimEnd('/');
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
        httpClient.DefaultRequestHeaders.Add("Authorization", options.ApiKey);

        return new StructClient(httpClient, baseUrl);
    }
}
