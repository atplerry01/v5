using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Channel;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.StreamCore.Channel;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingStreamCoreChannelSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ChannelCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.ChannelCreatedEvent), typeof(ChannelCreatedEventSchema));
        sink.RegisterSchema("ChannelRenamedEvent", EventVersion.Default,
            typeof(DomainEvents.ChannelRenamedEvent), typeof(ChannelRenamedEventSchema));
        sink.RegisterSchema("ChannelEnabledEvent", EventVersion.Default,
            typeof(DomainEvents.ChannelEnabledEvent), typeof(ChannelEnabledEventSchema));
        sink.RegisterSchema("ChannelDisabledEvent", EventVersion.Default,
            typeof(DomainEvents.ChannelDisabledEvent), typeof(ChannelDisabledEventSchema));
        sink.RegisterSchema("ChannelArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.ChannelArchivedEvent), typeof(ChannelArchivedEventSchema));

        sink.RegisterPayloadMapper("ChannelCreatedEvent", e =>
        {
            var evt = (DomainEvents.ChannelCreatedEvent)e;
            return new ChannelCreatedEventSchema(evt.ChannelId.Value, evt.StreamRef.Value, evt.Name.Value, evt.Mode.ToString(), evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("ChannelRenamedEvent", e =>
        {
            var evt = (DomainEvents.ChannelRenamedEvent)e;
            return new ChannelRenamedEventSchema(evt.ChannelId.Value, evt.PreviousName.Value, evt.NewName.Value, evt.RenamedAt.Value);
        });
        sink.RegisterPayloadMapper("ChannelEnabledEvent", e =>
        {
            var evt = (DomainEvents.ChannelEnabledEvent)e;
            return new ChannelEnabledEventSchema(evt.ChannelId.Value, evt.EnabledAt.Value);
        });
        sink.RegisterPayloadMapper("ChannelDisabledEvent", e =>
        {
            var evt = (DomainEvents.ChannelDisabledEvent)e;
            return new ChannelDisabledEventSchema(evt.ChannelId.Value, evt.Reason, evt.DisabledAt.Value);
        });
        sink.RegisterPayloadMapper("ChannelArchivedEvent", e =>
        {
            var evt = (DomainEvents.ChannelArchivedEvent)e;
            return new ChannelArchivedEventSchema(evt.ChannelId.Value, evt.ArchivedAt.Value);
        });
    }
}
