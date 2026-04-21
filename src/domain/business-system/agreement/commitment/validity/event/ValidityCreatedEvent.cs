using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Validity;

public sealed record ValidityCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ValidityId ValidityId) : DomainEvent;
