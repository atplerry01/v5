namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed record PolicyActivationRecord
{
    public required Guid PolicyId { get; init; }
    public required int Version { get; init; }
    public required IReadOnlyList<Guid> ActivatedBy { get; init; }
    public required string ActivationHash { get; init; }
    public required DateTimeOffset ActivatedAt { get; init; }
}
