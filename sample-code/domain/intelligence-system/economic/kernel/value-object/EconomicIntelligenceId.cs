using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.IntelligenceSystem.Economic;

public sealed record EconomicIntelligenceId(Guid Value)
{
    public static EconomicIntelligenceId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static EconomicIntelligenceId From(Guid id) => new(id);
    public static readonly EconomicIntelligenceId Empty = new(Guid.Empty);
}
