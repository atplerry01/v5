using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public sealed record QuoteIssuedEvent(
    [property: JsonPropertyName("AggregateId")] QuoteId QuoteId,
    TimeWindow Validity) : DomainEvent;
