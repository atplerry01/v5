using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

namespace Whycespace.Domain.StructuralSystem.Structure.RelationshipRules;

public sealed class SpvScopeConstraint
{
    public IReadOnlySet<SpvType> SubclusterScopedTypes { get; }

    public SpvScopeConstraint(IReadOnlySet<SpvType> subclusterScopedTypes)
    {
        if (subclusterScopedTypes is null)
            throw new ArgumentNullException(nameof(subclusterScopedTypes));

        SubclusterScopedTypes = subclusterScopedTypes;
    }

    public bool AllowsUnderSubcluster(SpvType type) => SubclusterScopedTypes.Contains(type);

    public static SpvScopeConstraint Permissive { get; } =
        new(new HashSet<SpvType>(Enum.GetValues<SpvType>()));
}
