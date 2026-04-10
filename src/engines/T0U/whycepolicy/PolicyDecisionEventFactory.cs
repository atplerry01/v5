using Whyce.Shared.Contracts.Policy;
using Whyce.Shared.Contracts.Runtime;
using Whycespace.Domain.ConstitutionalSystem.Policy.Decision;

namespace Whyce.Engines.T0U.WhycePolicy;

/// <summary>
/// Engine-layer factory for policy decision <see cref="AuditEmission"/> objects.
/// Lives here because engines are permitted to reference
/// <c>Whycespace.Domain.*</c> while runtime middleware is not (rule 11.R-DOM-01).
///
/// Returns a fully-formed AuditEmission carrying the event, dedicated routing
/// metadata (constitutional / policy / decision), and the mandatory
/// audit metadata bundle (DecisionHash, ExecutionHash, PolicyVersion, CommandId).
///
/// Pure: no clock, no RNG, no Guid.NewGuid — every field is sourced from
/// caller-supplied parameters that themselves originate from upstream context.
/// </summary>
public sealed class PolicyDecisionEventFactory : IPolicyDecisionEventFactory
{
    private const string AuditClassification = "constitutional";
    private const string AuditContext = "policy";
    private const string AuditDomain = "decision";

    public AuditEmission CreateEvaluatedEmission(
        Guid eventId,
        Guid aggregateId,
        string identityId,
        string policyName,
        string decisionHash,
        string executionHash,
        string policyVersion,
        Guid commandId,
        Guid correlationId,
        Guid causationId)
    {
        var evt = new PolicyEvaluatedEvent
        {
            EventId = eventId,
            IdentityId = identityId,
            PolicyName = policyName,
            IsAllowed = true,
            DecisionHash = decisionHash,
            CorrelationId = correlationId,
            CausationId = causationId
        };

        return BuildEmission(evt, aggregateId, decisionHash, executionHash, policyVersion, commandId);
    }

    public AuditEmission CreateDeniedEmission(
        Guid eventId,
        Guid aggregateId,
        string identityId,
        string policyName,
        string decisionHash,
        string executionHash,
        string policyVersion,
        Guid commandId,
        Guid correlationId,
        Guid causationId)
    {
        var evt = new PolicyDeniedEvent
        {
            EventId = eventId,
            IdentityId = identityId,
            PolicyName = policyName,
            DecisionHash = decisionHash,
            CorrelationId = correlationId,
            CausationId = causationId
        };

        return BuildEmission(evt, aggregateId, decisionHash, executionHash, policyVersion, commandId);
    }

    private static AuditEmission BuildEmission(
        object evt,
        Guid aggregateId,
        string decisionHash,
        string executionHash,
        string policyVersion,
        Guid commandId) =>
        new()
        {
            Events = new[] { evt },
            AggregateId = aggregateId,
            Classification = AuditClassification,
            Context = AuditContext,
            Domain = AuditDomain,
            Metadata = new Dictionary<string, string>
            {
                ["DecisionHash"] = decisionHash,
                ["ExecutionHash"] = executionHash,
                ["PolicyVersion"] = policyVersion,
                ["CommandId"] = commandId.ToString("N")
            }
        };
}
