using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed record SurchargeUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] SurchargeId SurchargeId,
    SurchargeName Name,
    AdjustmentBasis Basis,
    SurchargeAmount Amount) : DomainEvent;
