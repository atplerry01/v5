using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeId(Guid Value)
{
    public static ChargeId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static implicit operator Guid(ChargeId id) => id.Value;
}
