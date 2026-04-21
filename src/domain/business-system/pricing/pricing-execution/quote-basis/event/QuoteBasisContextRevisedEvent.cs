using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public sealed record QuoteBasisContextRevisedEvent(
    [property: JsonPropertyName("AggregateId")] QuoteBasisId QuoteBasisId,
    QuoteBasisContext Context) : DomainEvent;
