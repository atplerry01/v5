namespace Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;

public sealed record ServiceLevelReadModel
{
    public Guid ServiceLevelId { get; init; }
    public Guid ServiceDefinitionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Target { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
