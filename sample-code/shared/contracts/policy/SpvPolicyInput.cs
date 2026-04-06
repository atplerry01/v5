namespace Whycespace.Shared.Contracts.Policy;

public sealed record SpvPolicyInput
{
    public required string IdentityId { get; init; }
    public required Guid EntityId { get; init; }
    public required string Jurisdiction { get; init; }
}
