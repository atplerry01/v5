using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventStream;

public sealed record EventStreamDeclaredEvent(
    [property: JsonPropertyName("AggregateId")] EventStreamId EventStreamId,
    DomainRoute SourceRoute,
    IReadOnlyList<string> IncludedEventTypes,
    OrderingGuarantee OrderingGuarantee,
    Timestamp DeclaredAt) : DomainEvent;
