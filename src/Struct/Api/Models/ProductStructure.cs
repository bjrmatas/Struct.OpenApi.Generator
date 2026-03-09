namespace Struct.Api.Models;

public sealed record ProductStructureSummary(string Uid, string Alias, string Label);

public sealed record ProductStructure(
    string Uid,
    string Alias,
    string Label,
    bool HasVariants,
    bool HasVariantGroups,
    ProductConfiguration? ProductConfiguration,
    VariantConfiguration? VariantConfiguration);

public sealed record ProductConfiguration(List<Tab> Tabs);

public sealed record VariantConfiguration(List<Tab> Tabs);

public sealed record Tab(string Uid, string Label, List<Section> Sections, string? Type);

public sealed record Section(string Uid, string Headline, List<Property> Properties);

public sealed record Property(string Uid, string? AttributeUid, string? Type);
