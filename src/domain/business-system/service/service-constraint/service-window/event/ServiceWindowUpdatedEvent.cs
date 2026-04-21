using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceWindowId ServiceWindowId,
    TimeWindow Range) : DomainEvent;
