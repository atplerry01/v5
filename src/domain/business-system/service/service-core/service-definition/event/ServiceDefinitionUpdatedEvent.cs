namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceDefinition;

public sealed record ServiceDefinitionUpdatedEvent(
    ServiceDefinitionId ServiceDefinitionId,
    ServiceDefinitionName Name,
    ServiceCategory Category);
