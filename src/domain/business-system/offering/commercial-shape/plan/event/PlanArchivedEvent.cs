using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed record PlanArchivedEvent(
    [property: JsonPropertyName("AggregateId")] PlanId PlanId) : DomainEvent;
