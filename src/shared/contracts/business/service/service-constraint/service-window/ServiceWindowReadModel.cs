namespace Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowReadModel
{
    public Guid ServiceWindowId { get; init; }
    public Guid ServiceDefinitionId { get; init; }
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
