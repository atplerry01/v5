namespace Whycespace.Shared.Contracts.Events.Platform.Event.EventStream;

public sealed record EventStreamDeclaredEventSchema(
    Guid AggregateId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    IReadOnlyList<string> IncludedEventTypes,
    string OrderingGuarantee);
