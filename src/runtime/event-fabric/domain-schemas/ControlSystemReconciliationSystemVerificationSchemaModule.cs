using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.SystemVerification;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemReconciliationSystemVerificationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SystemVerificationInitiatedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemVerificationInitiatedEvent), typeof(SystemVerificationInitiatedEventSchema));
        sink.RegisterSchema("SystemVerificationPassedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemVerificationPassedEvent), typeof(SystemVerificationPassedEventSchema));
        sink.RegisterSchema("SystemVerificationFailedEvent", EventVersion.Default,
            typeof(DomainEvents.SystemVerificationFailedEvent), typeof(SystemVerificationFailedEventSchema));

        sink.RegisterPayloadMapper("SystemVerificationInitiatedEvent", e =>
        {
            var evt = (DomainEvents.SystemVerificationInitiatedEvent)e;
            return new SystemVerificationInitiatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.TargetSystem,
                evt.InitiatedAt);
        });
        sink.RegisterPayloadMapper("SystemVerificationPassedEvent", e =>
        {
            var evt = (DomainEvents.SystemVerificationPassedEvent)e;
            return new SystemVerificationPassedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.PassedAt);
        });
        sink.RegisterPayloadMapper("SystemVerificationFailedEvent", e =>
        {
            var evt = (DomainEvents.SystemVerificationFailedEvent)e;
            return new SystemVerificationFailedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.FailureReason,
                evt.FailedAt);
        });
    }
}
