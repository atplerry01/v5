namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed record ServiceDefinitionCreatedEvent(
    ServiceDefinitionId ServiceDefinitionId,
    ServiceDefinitionName Name,
    ServiceCategory Category);
