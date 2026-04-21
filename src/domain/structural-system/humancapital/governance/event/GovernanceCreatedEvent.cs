using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Governance;

public sealed record GovernanceCreatedEvent(
    [property: JsonPropertyName("AggregateId")] GovernanceId GovernanceId,
    GovernanceDescriptor Descriptor) : DomainEvent;
