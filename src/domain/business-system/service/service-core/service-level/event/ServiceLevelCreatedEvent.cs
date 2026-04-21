using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed record ServiceLevelCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceLevelId ServiceLevelId,
    ServiceDefinitionRef ServiceDefinition,
    LevelCode Code,
    LevelName Name,
    ServiceLevelTarget Target) : DomainEvent;
