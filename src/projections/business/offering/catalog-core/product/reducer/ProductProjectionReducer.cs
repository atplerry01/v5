using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Product;

namespace Whycespace.Projections.Business.Offering.CatalogCore.Product.Reducer;

public static class ProductProjectionReducer
{
    public static ProductReadModel Apply(ProductReadModel state, ProductCreatedEventSchema e) =>
        state with
        {
            ProductId = e.AggregateId,
            Name = e.Name,
            Type = e.Type,
            CatalogId = e.CatalogId,
            Status = "Draft"
        };

    public static ProductReadModel Apply(ProductReadModel state, ProductUpdatedEventSchema e) =>
        state with
        {
            ProductId = e.AggregateId,
            Name = e.Name,
            Type = e.Type
        };

    public static ProductReadModel Apply(ProductReadModel state, ProductActivatedEventSchema e) =>
        state with
        {
            ProductId = e.AggregateId,
            Status = "Active"
        };

    public static ProductReadModel Apply(ProductReadModel state, ProductArchivedEventSchema e) =>
        state with
        {
            ProductId = e.AggregateId,
            Status = "Archived"
        };
}
