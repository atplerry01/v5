using Whycespace.Shared.Primitives.Money;

namespace Whycespace.Shared.Contracts.Policy;

public sealed record CrossSpvPolicyInput
{
    public required string IdentityId { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid RootSpvId { get; init; }
    public required IReadOnlyList<SpvLegPolicy> Legs { get; init; }
    public required Money TotalAmount { get; init; }
    public required string Jurisdiction { get; init; }
}

public sealed record SpvLegPolicy
{
    public required Guid FromSpvId { get; init; }
    public required Guid ToSpvId { get; init; }
    public required Money Amount { get; init; }
}
