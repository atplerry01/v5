using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;

public sealed record CreateProductCommand(
    Guid ProductId,
    string Name,
    string Type,
    Guid? CatalogId) : IHasAggregateId
{
    public Guid AggregateId => ProductId;
}

public sealed record UpdateProductCommand(
    Guid ProductId,
    string Name,
    string Type) : IHasAggregateId
{
    public Guid AggregateId => ProductId;
}

public sealed record ActivateProductCommand(Guid ProductId) : IHasAggregateId
{
    public Guid AggregateId => ProductId;
}

public sealed record ArchiveProductCommand(Guid ProductId) : IHasAggregateId
{
    public Guid AggregateId => ProductId;
}
