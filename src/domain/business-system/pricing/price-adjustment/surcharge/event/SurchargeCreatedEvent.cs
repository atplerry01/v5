using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Pricing;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PriceAdjustment.Surcharge;

public sealed record SurchargeCreatedEvent(
    [property: JsonPropertyName("AggregateId")] SurchargeId SurchargeId,
    SurchargeCode Code,
    SurchargeName Name,
    AdjustmentBasis Basis,
    SurchargeAmount Amount) : DomainEvent;
