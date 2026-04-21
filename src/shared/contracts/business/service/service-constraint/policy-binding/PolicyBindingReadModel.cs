namespace Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingReadModel
{
    public Guid PolicyBindingId { get; init; }
    public Guid ServiceDefinitionId { get; init; }
    public string PolicyRef { get; init; } = string.Empty;
    public int Scope { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? BoundAt { get; init; }
    public DateTimeOffset? UnboundAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
