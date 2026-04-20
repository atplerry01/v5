using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.Tariff;

public sealed record TariffCreatedEvent(
    TariffId TariffId,
    PriceBookRef PriceBook,
    TariffCode Code,
    TariffName Name);
