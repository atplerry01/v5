namespace Whycespace.Shared.Contracts.Policy;

public sealed record SubClusterPolicyInput
{
    public required string IdentityId { get; init; }
    public required Guid EntityId { get; init; }
    public required string Jurisdiction { get; init; }
}
