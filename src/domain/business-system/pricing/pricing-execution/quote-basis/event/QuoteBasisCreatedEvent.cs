using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed record QuoteBasisCreatedEvent(
    [property: JsonPropertyName("AggregateId")] QuoteBasisId QuoteBasisId,
    PriceBookRef PriceBook,
    QuoteBasisContext Context) : DomainEvent;
