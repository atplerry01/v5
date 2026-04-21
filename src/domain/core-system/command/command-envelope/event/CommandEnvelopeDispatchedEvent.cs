using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed record CommandEnvelopeDispatchedEvent(
    [property: JsonPropertyName("AggregateId")] CommandEnvelopeId EnvelopeId) : DomainEvent;
