using DomainEvents = Whycespace.Domain.ConstitutionalSystem.Policy.Decision;

namespace Whyce.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the constitutional/policy/decision domain.
///
/// Registers <c>PolicyEvaluatedEvent</c> and <c>PolicyDeniedEvent</c> with
/// stored type == inbound type (no shared-contract schema record exists for
/// these yet — that gap is tracked separately and intentionally preserved by
/// this remediation). Relocated from
/// <c>src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs</c>
/// under Phase 1.5 §5.1.2 BPV-D01.
/// </summary>
public sealed class PolicyDecisionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "PolicyEvaluatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PolicyEvaluatedEvent),
            typeof(DomainEvents.PolicyEvaluatedEvent));

        sink.RegisterSchema(
            "PolicyDeniedEvent",
            EventVersion.Default,
            typeof(DomainEvents.PolicyDeniedEvent),
            typeof(DomainEvents.PolicyDeniedEvent));
    }
}
