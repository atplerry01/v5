using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public sealed record ParticipantPlacedEvent(
    [property: JsonPropertyName("AggregateId")] string ParticipantId,
    ClusterRef HomeCluster,
    DateTimeOffset EffectiveAt) : DomainEvent;
