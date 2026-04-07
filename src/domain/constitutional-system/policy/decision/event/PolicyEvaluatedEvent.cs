namespace Whycespace.Domain.ConstitutionalSystem.Policy.Decision;

/// <summary>
/// Domain event recording an ALLOW policy decision.
///
/// Emitted by the runtime policy middleware after WHYCEPOLICY evaluation
/// returns IsCompliant=true. The DecisionHash MUST be the hash produced by
/// the policy engine — never regenerated. All identifiers are sourced from
/// upstream context (CommandContext / authenticated identity); this event
/// MUST NOT call IIdGenerator, IClock, Guid.NewGuid, or DateTime.UtcNow.
///
/// Determinism: replay reproduces the identical record because every field
/// is sourced from previously-persisted upstream state.
/// </summary>
public sealed record PolicyEvaluatedEvent
{
    public required Guid EventId { get; init; }
    public required string IdentityId { get; init; }
    public required string PolicyName { get; init; }
    public required bool IsAllowed { get; init; }
    public required string DecisionHash { get; init; }
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
}
