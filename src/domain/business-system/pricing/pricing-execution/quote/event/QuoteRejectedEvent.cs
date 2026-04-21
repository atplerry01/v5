using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteRejectedEvent(
    [property: JsonPropertyName("AggregateId")] QuoteId QuoteId,
    DateTimeOffset RejectedAt) : DomainEvent;
