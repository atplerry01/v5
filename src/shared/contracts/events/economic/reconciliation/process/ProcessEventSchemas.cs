namespace Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Process;

// Schema property names mirror the domain event property names
// (ProcessId, LedgerReference, ObservedReference, TriggeredAt) so the
// stored payload round-trips cleanly through the event deserializer's
// value-object converters on replay.

public sealed record ReconciliationTriggeredEventSchema(
    Guid ProcessId,
    Guid LedgerReference,
    Guid ObservedReference,
    DateTimeOffset TriggeredAt);

public sealed record ReconciliationMatchedEventSchema(Guid ProcessId);

public sealed record ReconciliationMismatchedEventSchema(Guid ProcessId);

public sealed record ReconciliationResolvedEventSchema(Guid ProcessId);
