using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession;

public sealed record StreamOpenedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    StreamSessionId SessionId, string AssetRef, string EndpointUri, Timestamp OpenedAt) : DomainEvent;
