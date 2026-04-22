using Whycespace.Shared.Contracts.Events.Trust.Identity.IdentityGraph;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.IdentityGraph;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityIdentityGraphSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("IdentityGraphInitializedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentityGraphInitializedEvent), typeof(IdentityGraphInitializedEventSchema));
        sink.RegisterSchema("IdentityGraphArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentityGraphArchivedEvent), typeof(IdentityGraphArchivedEventSchema));

        sink.RegisterPayloadMapper("IdentityGraphInitializedEvent", e =>
        {
            var evt = (DomainEvents.IdentityGraphInitializedEvent)e;
            return new IdentityGraphInitializedEventSchema(
                evt.IdentityGraphId.Value,
                evt.Descriptor.PrimaryIdentityReference,
                evt.Descriptor.GraphContext,
                evt.InitializedAt.Value);
        });
        sink.RegisterPayloadMapper("IdentityGraphArchivedEvent", e =>
        {
            var evt = (DomainEvents.IdentityGraphArchivedEvent)e;
            return new IdentityGraphArchivedEventSchema(evt.IdentityGraphId.Value);
        });
    }
}
