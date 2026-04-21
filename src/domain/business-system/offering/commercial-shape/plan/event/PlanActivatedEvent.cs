using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed record PlanActivatedEvent(
    [property: JsonPropertyName("AggregateId")] PlanId PlanId) : DomainEvent;
