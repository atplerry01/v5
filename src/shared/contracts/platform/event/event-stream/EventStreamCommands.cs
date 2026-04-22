using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Event.EventStream;

public sealed record DeclareEventStreamCommand(
    Guid EventStreamId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    IReadOnlyList<string> IncludedEventTypes,
    string OrderingGuarantee,
    DateTimeOffset DeclaredAt) : IHasAggregateId
{
    public Guid AggregateId => EventStreamId;
}
