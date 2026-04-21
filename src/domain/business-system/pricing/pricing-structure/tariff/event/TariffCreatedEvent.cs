using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed record TariffCreatedEvent(
    [property: JsonPropertyName("AggregateId")] TariffId TariffId,
    PriceBookRef PriceBook,
    TariffCode Code,
    TariffName Name) : DomainEvent;
