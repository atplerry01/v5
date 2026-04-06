using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureId(Guid Value)
{
    public static StructureId CreateDeterministic(string seed)
        => new(DeterministicIdHelper.FromSeed(seed));

    public static readonly StructureId Empty = new(Guid.Empty);
}
