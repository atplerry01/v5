namespace Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Discrepancy;

// Schema property names mirror the domain event property names
// (DiscrepancyId, ProcessReference, …) so the stored payload round-trips
// cleanly through the event deserializer's value-object converters on
// replay.

public sealed record DiscrepancyDetectedEventSchema(
    Guid DiscrepancyId,
    Guid ProcessReference,
    string Source,
    decimal ExpectedValue,
    decimal ActualValue,
    decimal Difference,
    DateTimeOffset DetectedAt);

public sealed record DiscrepancyInvestigatedEventSchema(Guid DiscrepancyId);

public sealed record DiscrepancyResolvedEventSchema(Guid DiscrepancyId, string Resolution);
