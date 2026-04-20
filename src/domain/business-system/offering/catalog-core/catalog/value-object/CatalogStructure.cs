namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public readonly record struct CatalogStructure
{
    public string Name { get; }
    public string Category { get; }

    public CatalogStructure(string name, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Catalog name must not be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Catalog category must not be empty.", nameof(category));

        Name = name;
        Category = category;
    }
}
