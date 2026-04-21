using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed record EventEnvelopePublishedEvent(
    [property: JsonPropertyName("AggregateId")] EventEnvelopeId EnvelopeId) : DomainEvent;
