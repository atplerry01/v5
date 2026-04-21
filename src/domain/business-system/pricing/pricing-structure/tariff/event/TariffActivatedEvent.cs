using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed record TariffActivatedEvent(
    [property: JsonPropertyName("AggregateId")] TariffId TariffId,
    TimeWindow Effective) : DomainEvent;
