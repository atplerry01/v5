using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Incentive;

public sealed record IncentiveCreatedEvent(
    [property: JsonPropertyName("AggregateId")] IncentiveId IncentiveId,
    IncentiveDescriptor Descriptor) : DomainEvent;
