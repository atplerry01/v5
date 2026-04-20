namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public sealed record ServiceLevelUpdatedEvent(
    ServiceLevelId ServiceLevelId,
    LevelName Name,
    ServiceLevelTarget Target);
