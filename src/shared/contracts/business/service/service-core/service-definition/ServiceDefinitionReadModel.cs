namespace Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;

public sealed record ServiceDefinitionReadModel
{
    public Guid ServiceDefinitionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
