using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Humancapital.Participant;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Assignment;

public sealed record AssignmentAssignedEvent(
    [property: JsonPropertyName("AggregateId")] AssignmentId AssignmentId,
    ParticipantId Participant,
    ClusterAuthorityRef Authority,
    DateTimeOffset EffectiveAt) : DomainEvent;
