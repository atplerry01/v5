using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public readonly record struct CatalogStructure
{
    public string Name { get; }
    public string Category { get; }

    public CatalogStructure(string name, string category)
    {
        Guard.Against(string.IsNullOrWhiteSpace(name), "Catalog name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(category), "Catalog category must not be empty.");

        Name = name!;
        Category = category!;
    }
}
