namespace Struct.Api.Models;

public static class ProductStructureExtensions
{
    /// <summary>
    /// Extracts all unique attribute UIDs from a product structure's tabs and sections.
    /// </summary>
    public static (List<string> ProductAttributes, List<string> VariantAttributes) ExtractAttributeUids(this ProductStructure structure)
    {
        var productUids = new HashSet<string>();
        var variantUids = new HashSet<string>();

        if (structure.ProductConfiguration?.Tabs != null)
        {
            foreach (var tab in structure.ProductConfiguration.Tabs)
            {
                if (tab.Sections == null) continue;
                foreach (var section in tab.Sections)
                {
                    if (section.Properties == null) continue;
                    foreach (var prop in section.Properties)
                    {
                        if (!string.IsNullOrEmpty(prop.AttributeUid))
                        {
                            productUids.Add(prop.AttributeUid);
                        }
                    }
                }
            }
        }

        if (structure.VariantConfiguration?.Tabs != null)
        {
            foreach (var tab in structure.VariantConfiguration.Tabs)
            {
                if (tab.Sections == null) continue;
                foreach (var section in tab.Sections)
                {
                    if (section.Properties == null) continue;
                    foreach (var prop in section.Properties)
                    {
                        if (!string.IsNullOrEmpty(prop.AttributeUid))
                        {
                            variantUids.Add(prop.AttributeUid);
                        }
                    }
                }
            }
        }

        return ([.. productUids], [.. variantUids]);
    }
}
