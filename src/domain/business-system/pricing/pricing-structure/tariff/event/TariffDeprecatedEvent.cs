using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed record TariffDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] TariffId TariffId) : DomainEvent;
