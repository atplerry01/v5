using Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.PolicyBinding;
using DomainEvents = Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/service/service-constraint/policy-binding domain.
/// </summary>
public sealed class BusinessServiceServiceConstraintPolicyBindingSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "PolicyBindingCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PolicyBindingCreatedEvent),
            typeof(PolicyBindingCreatedEventSchema));

        sink.RegisterSchema(
            "PolicyBindingBoundEvent",
            EventVersion.Default,
            typeof(DomainEvents.PolicyBindingBoundEvent),
            typeof(PolicyBindingBoundEventSchema));

        sink.RegisterSchema(
            "PolicyBindingUnboundEvent",
            EventVersion.Default,
            typeof(DomainEvents.PolicyBindingUnboundEvent),
            typeof(PolicyBindingUnboundEventSchema));

        sink.RegisterSchema(
            "PolicyBindingArchivedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PolicyBindingArchivedEvent),
            typeof(PolicyBindingArchivedEventSchema));

        sink.RegisterPayloadMapper("PolicyBindingCreatedEvent", e =>
        {
            var evt = (DomainEvents.PolicyBindingCreatedEvent)e;
            return new PolicyBindingCreatedEventSchema(
                evt.PolicyBindingId.Value,
                evt.ServiceDefinition.Value,
                evt.Policy.Value,
                (int)evt.Scope);
        });
        sink.RegisterPayloadMapper("PolicyBindingBoundEvent", e =>
        {
            var evt = (DomainEvents.PolicyBindingBoundEvent)e;
            return new PolicyBindingBoundEventSchema(evt.PolicyBindingId.Value, evt.BoundAt);
        });
        sink.RegisterPayloadMapper("PolicyBindingUnboundEvent", e =>
        {
            var evt = (DomainEvents.PolicyBindingUnboundEvent)e;
            return new PolicyBindingUnboundEventSchema(evt.PolicyBindingId.Value, evt.UnboundAt);
        });
        sink.RegisterPayloadMapper("PolicyBindingArchivedEvent", e =>
        {
            var evt = (DomainEvents.PolicyBindingArchivedEvent)e;
            return new PolicyBindingArchivedEventSchema(evt.PolicyBindingId.Value);
        });
    }
}
