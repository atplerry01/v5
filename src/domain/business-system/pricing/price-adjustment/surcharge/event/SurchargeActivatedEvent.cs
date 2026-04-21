using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed record SurchargeActivatedEvent(
    [property: JsonPropertyName("AggregateId")] SurchargeId SurchargeId) : DomainEvent;
