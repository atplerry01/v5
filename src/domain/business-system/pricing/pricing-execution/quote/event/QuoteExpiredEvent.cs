using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteExpiredEvent(
    [property: JsonPropertyName("AggregateId")] QuoteId QuoteId,
    DateTimeOffset ExpiredAt) : DomainEvent;
