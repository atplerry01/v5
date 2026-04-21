namespace Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionReadModel
{
    public Guid ServiceOptionId { get; init; }
    public Guid ServiceDefinitionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
