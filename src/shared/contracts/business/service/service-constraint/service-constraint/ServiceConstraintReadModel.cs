namespace Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintReadModel
{
    public Guid ServiceConstraintId { get; init; }
    public Guid ServiceDefinitionId { get; init; }
    public int Kind { get; init; }
    public string Descriptor { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastUpdatedAt { get; init; }
}
