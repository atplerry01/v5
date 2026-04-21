using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.UsageRight;

public sealed record UsageRightConsumedEvent(
    [property: JsonPropertyName("AggregateId")] UsageRightId UsageRightId) : DomainEvent;
