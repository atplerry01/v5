using Whycespace.Shared.Contracts.Events.Platform.Schema.Versioning;
using DomainEvents = Whycespace.Domain.PlatformSystem.Schema.Versioning;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformVersioningRuleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("VersioningRuleRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.VersioningRuleRegisteredEvent), typeof(VersioningRuleRegisteredEventSchema));
        sink.RegisterSchema("VersioningRuleVerdictIssuedEvent", EventVersion.Default,
            typeof(DomainEvents.VersioningRuleVerdictIssuedEvent), typeof(VersioningRuleVerdictIssuedEventSchema));

        sink.RegisterPayloadMapper("VersioningRuleRegisteredEvent", e =>
        {
            var evt = (DomainEvents.VersioningRuleRegisteredEvent)e;
            return new VersioningRuleRegisteredEventSchema(
                evt.VersioningRuleId.Value,
                evt.SchemaRef,
                evt.FromVersion,
                evt.ToVersion,
                evt.EvolutionClass.Value);
        });
        sink.RegisterPayloadMapper("VersioningRuleVerdictIssuedEvent", e =>
        {
            var evt = (DomainEvents.VersioningRuleVerdictIssuedEvent)e;
            return new VersioningRuleVerdictIssuedEventSchema(
                evt.VersioningRuleId.Value,
                evt.Verdict.Value);
        });
    }
}
