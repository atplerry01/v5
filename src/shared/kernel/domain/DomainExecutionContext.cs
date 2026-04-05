namespace Whycespace.Shared.Kernel.Domain;

public sealed record DomainExecutionContext
{
    public required Guid CorrelationId { get; init; }
    public required Guid CausationId { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
