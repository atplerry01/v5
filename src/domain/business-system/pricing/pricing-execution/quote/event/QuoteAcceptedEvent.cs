using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteAcceptedEvent(
    [property: JsonPropertyName("AggregateId")] QuoteId QuoteId,
    DateTimeOffset AcceptedAt) : DomainEvent;
