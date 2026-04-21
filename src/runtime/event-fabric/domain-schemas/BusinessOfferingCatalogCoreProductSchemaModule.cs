using Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Product;
using DomainEvents = Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/offering/catalog-core/product domain.
///
/// Owns the binding from Product domain event CLR types to the
/// <see cref="EventSchemaRegistry"/>, plus the outbound payload mappers that
/// project domain events (with strongly-typed ProductId / ProductName /
/// ProductType / CatalogRef value-objects) into the shared schema records
/// (Guid AggregateId + flattened primitives) consumed by the projection layer.
/// ProductType is serialized as the enum name so the wire schema stays stable
/// across replay.
/// </summary>
public sealed class BusinessOfferingCatalogCoreProductSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ProductCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProductCreatedEvent),
            typeof(ProductCreatedEventSchema));

        sink.RegisterSchema(
            "ProductUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProductUpdatedEvent),
            typeof(ProductUpdatedEventSchema));

        sink.RegisterSchema(
            "ProductActivatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProductActivatedEvent),
            typeof(ProductActivatedEventSchema));

        sink.RegisterSchema(
            "ProductArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ProductArchivedEvent),
            typeof(ProductArchivedEventSchema));

        sink.RegisterPayloadMapper("ProductCreatedEvent", e =>
        {
            var evt = (DomainEvents.ProductCreatedEvent)e;
            return new ProductCreatedEventSchema(
                evt.ProductId.Value,
                evt.Name.Value,
                evt.Type.ToString(),
                evt.Catalog?.Value);
        });
        sink.RegisterPayloadMapper("ProductUpdatedEvent", e =>
        {
            var evt = (DomainEvents.ProductUpdatedEvent)e;
            return new ProductUpdatedEventSchema(
                evt.ProductId.Value,
                evt.Name.Value,
                evt.Type.ToString());
        });
        sink.RegisterPayloadMapper("ProductActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProductActivatedEvent)e;
            return new ProductActivatedEventSchema(evt.ProductId.Value);
        });
        sink.RegisterPayloadMapper("ProductArchivedEvent", e =>
        {
            var evt = (DomainEvents.ProductArchivedEvent)e;
            return new ProductArchivedEventSchema(evt.ProductId.Value);
        });
    }
}
