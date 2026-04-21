using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceWindowId ServiceWindowId,
    ServiceDefinitionRef ServiceDefinition,
    TimeWindow Range) : DomainEvent;
