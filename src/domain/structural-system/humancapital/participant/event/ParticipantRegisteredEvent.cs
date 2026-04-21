using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public sealed record ParticipantRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] string ParticipantId) : DomainEvent;
