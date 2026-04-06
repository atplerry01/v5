using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobId(Guid Value)
{
    public static JobId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
}
