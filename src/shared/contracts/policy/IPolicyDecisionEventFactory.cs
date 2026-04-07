using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Shared.Contracts.Policy;

/// <summary>
/// Constructs a policy decision <see cref="AuditEmission"/> from primitive
/// coordinates. Returns the full audit emission (events + routing + metadata)
/// rather than raw events so the runtime middleware never has to assemble
/// audit envelopes itself.
///
/// Exists so that runtime middleware (which is forbidden by rule 11.R-DOM-01
/// from referencing concrete <c>Whycespace.Domain.*</c> types) can produce
/// policy events without coupling to the domain assembly. Implementations
/// live in the engines layer where domain references are permitted.
///
/// All factory inputs MUST originate from upstream context — never from
/// IClock, IIdGenerator, or any non-deterministic source inside the factory.
/// The factory is a pure constructor over its parameters.
/// </summary>
public interface IPolicyDecisionEventFactory
{
    AuditEmission CreateEvaluatedEmission(
        Guid eventId,
        Guid aggregateId,
        string identityId,
        string policyName,
        string decisionHash,
        string executionHash,
        string policyVersion,
        Guid commandId,
        Guid correlationId,
        Guid causationId);

    AuditEmission CreateDeniedEmission(
        Guid eventId,
        Guid aggregateId,
        string identityId,
        string policyName,
        string decisionHash,
        string executionHash,
        string policyVersion,
        Guid commandId,
        Guid correlationId,
        Guid causationId);
}
