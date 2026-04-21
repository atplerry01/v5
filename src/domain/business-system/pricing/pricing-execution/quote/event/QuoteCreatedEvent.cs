using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteCreatedEvent(
    [property: JsonPropertyName("AggregateId")] QuoteId QuoteId,
    QuoteBasisRef QuoteBasis,
    QuoteReference Reference) : DomainEvent;
