using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed record ServiceLevelCreatedEvent(
    ServiceLevelId ServiceLevelId,
    ServiceDefinitionRef ServiceDefinition,
    LevelCode Code,
    LevelName Name,
    ServiceLevelTarget Target);
