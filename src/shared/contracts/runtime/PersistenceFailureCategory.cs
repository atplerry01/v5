namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Persistence-boundary rejection taxonomy. Emitted when the event-store / outbox
/// / chain-anchor stack cannot complete an authoritative write.
///
/// See spec §10 Persistence Boundary Features.
/// </summary>
public enum PersistenceFailureCategory
{
    Unknown = 0,

    OptimisticConflict,
    Unavailable,
    Timeout,
    IntegrityViolation,
    PartialWrite,
    Exhausted,
    Duplicate
}
