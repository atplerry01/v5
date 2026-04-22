using Whycespace.Shared.Contracts.Events.Platform.Command.CommandRouting;
using DomainEvents = Whycespace.Domain.PlatformSystem.Command.CommandRouting;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformCommandRoutingSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("CommandRoutingRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.CommandRoutingRegisteredEvent), typeof(CommandRoutingRegisteredEventSchema));
        sink.RegisterSchema("CommandRoutingRemovedEvent", EventVersion.Default,
            typeof(DomainEvents.CommandRoutingRemovedEvent), typeof(CommandRoutingRemovedEventSchema));

        sink.RegisterPayloadMapper("CommandRoutingRegisteredEvent", e =>
        {
            var evt = (DomainEvents.CommandRoutingRegisteredEvent)e;
            return new CommandRoutingRegisteredEventSchema(evt.CommandRoutingRuleId.Value,
                evt.CommandTypeRef.CommandDefinitionId,
                evt.HandlerRoute.Classification, evt.HandlerRoute.Context, evt.HandlerRoute.Domain);
        });
        sink.RegisterPayloadMapper("CommandRoutingRemovedEvent", e =>
        {
            var evt = (DomainEvents.CommandRoutingRemovedEvent)e;
            return new CommandRoutingRemovedEventSchema(evt.CommandRoutingRuleId.Value);
        });
    }
}
