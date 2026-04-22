using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyPackage;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemPolicy.PolicyPackage;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemPolicyPolicyPackageSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PolicyPackageAssembledEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyPackageAssembledEvent), typeof(PolicyPackageAssembledEventSchema));
        sink.RegisterSchema("PolicyPackageDeployedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyPackageDeployedEvent), typeof(PolicyPackageDeployedEventSchema));
        sink.RegisterSchema("PolicyPackageRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyPackageRetiredEvent), typeof(PolicyPackageRetiredEventSchema));

        sink.RegisterPayloadMapper("PolicyPackageAssembledEvent", e =>
        {
            var evt = (DomainEvents.PolicyPackageAssembledEvent)e;
            return new PolicyPackageAssembledEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Version.Major,
                evt.Version.Minor,
                evt.PolicyDefinitionIds.ToList());
        });
        sink.RegisterPayloadMapper("PolicyPackageDeployedEvent", e =>
        {
            var evt = (DomainEvents.PolicyPackageDeployedEvent)e;
            return new PolicyPackageDeployedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Version.Major,
                evt.Version.Minor);
        });
        sink.RegisterPayloadMapper("PolicyPackageRetiredEvent", e =>
        {
            var evt = (DomainEvents.PolicyPackageRetiredEvent)e;
            return new PolicyPackageRetiredEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
