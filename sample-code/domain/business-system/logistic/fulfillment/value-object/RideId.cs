using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

/// <summary>
/// Strongly-typed identifier for a RideAggregate aggregate.
/// </summary>
public sealed record RideId(Guid Value)
{
    public static RideId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
}
