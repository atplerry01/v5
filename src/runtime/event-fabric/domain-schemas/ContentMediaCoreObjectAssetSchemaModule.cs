using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Asset;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaCoreObjectAssetSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AssetCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.AssetCreatedEvent), typeof(AssetCreatedEventSchema));
        sink.RegisterSchema("AssetRenamedEvent", EventVersion.Default,
            typeof(DomainEvents.AssetRenamedEvent), typeof(AssetRenamedEventSchema));
        sink.RegisterSchema("AssetReclassifiedEvent", EventVersion.Default,
            typeof(DomainEvents.AssetReclassifiedEvent), typeof(AssetReclassifiedEventSchema));
        sink.RegisterSchema("AssetActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.AssetActivatedEvent), typeof(AssetActivatedEventSchema));
        sink.RegisterSchema("AssetRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.AssetRetiredEvent), typeof(AssetRetiredEventSchema));
        sink.RegisterSchema("AssetKindAssignedEvent", EventVersion.Default,
            typeof(DomainEvents.AssetKindAssignedEvent), typeof(AssetKindAssignedEventSchema));

        sink.RegisterPayloadMapper("AssetCreatedEvent", e =>
        {
            var evt = (DomainEvents.AssetCreatedEvent)e;
            return new AssetCreatedEventSchema(evt.AssetId.Value, evt.Title.Value, evt.Classification.ToString(), evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("AssetRenamedEvent", e =>
        {
            var evt = (DomainEvents.AssetRenamedEvent)e;
            return new AssetRenamedEventSchema(evt.AssetId.Value, evt.PreviousTitle.Value, evt.NewTitle.Value, evt.RenamedAt.Value);
        });
        sink.RegisterPayloadMapper("AssetReclassifiedEvent", e =>
        {
            var evt = (DomainEvents.AssetReclassifiedEvent)e;
            return new AssetReclassifiedEventSchema(evt.AssetId.Value, evt.PreviousClassification.ToString(), evt.NewClassification.ToString(), evt.ReclassifiedAt.Value);
        });
        sink.RegisterPayloadMapper("AssetActivatedEvent", e =>
        {
            var evt = (DomainEvents.AssetActivatedEvent)e;
            return new AssetActivatedEventSchema(evt.AssetId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("AssetRetiredEvent", e =>
        {
            var evt = (DomainEvents.AssetRetiredEvent)e;
            return new AssetRetiredEventSchema(evt.AssetId.Value, evt.RetiredAt.Value);
        });
        sink.RegisterPayloadMapper("AssetKindAssignedEvent", e =>
        {
            var evt = (DomainEvents.AssetKindAssignedEvent)e;
            return new AssetKindAssignedEventSchema(evt.AssetId.Value, evt.PreviousKind.ToString(), evt.NewKind.ToString(), evt.AssignedAt.Value);
        });
    }
}
