namespace Whycespace.Domain.ConstitutionalSystem.Policy.Decision;

/// <summary>
/// Domain event recording a DENY policy decision.
///
/// Distinct from PolicyEvaluatedEvent — denial is explicit, never inferred
/// from IsAllowed=false on an allow event. This separation lets projections
/// and audit consumers subscribe to denials without filtering, and prevents
/// silent decisions.
///
/// Emitted by the runtime policy middleware when WHYCEPOLICY evaluation
/// returns IsCompliant=false. DecisionHash is the hash produced by the
/// policy engine — never regenerated. All identifiers are sourced from
/// upstream context. No clock, no RNG, no Guid.NewGuid.
/// </summary>
public sealed record PolicyDeniedEvent
{
    public required Guid EventId { get; init; }
    public required string IdentityId { get; init; }
    public required string PolicyName { get; init; }
    public required string DecisionHash { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
}
