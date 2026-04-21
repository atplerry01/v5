using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Limit;

public sealed record LimitBreachedEvent(
    [property: JsonPropertyName("AggregateId")] LimitId LimitId,
    int ObservedValue) : DomainEvent;
