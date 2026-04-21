using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Plan;

public sealed record PlanDraftedEvent(
    [property: JsonPropertyName("AggregateId")] PlanId PlanId,
    PlanDescriptor Descriptor) : DomainEvent;
