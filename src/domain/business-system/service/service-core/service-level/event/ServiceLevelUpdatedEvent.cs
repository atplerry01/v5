using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed record ServiceLevelUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceLevelId ServiceLevelId,
    LevelName Name,
    ServiceLevelTarget Target) : DomainEvent;
