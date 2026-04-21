using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

namespace Whycespace.Domain.StructuralSystem.Structure.RelationshipRules;

public sealed class AuthorityProviderMatrix
{
    public IReadOnlyDictionary<AuthorityRole, IReadOnlySet<ProviderCategory>> Allowed { get; }

    public AuthorityProviderMatrix(IReadOnlyDictionary<AuthorityRole, IReadOnlySet<ProviderCategory>> allowed)
    {
        if (allowed is null)
            throw new ArgumentNullException(nameof(allowed));

        Allowed = allowed;
    }

    public bool IsAllowed(AuthorityRole role, ProviderCategory category)
        => Allowed.TryGetValue(role, out var set) && set.Contains(category);

    public static AuthorityProviderMatrix Permissive { get; } = BuildPermissive();

    private static AuthorityProviderMatrix BuildPermissive()
    {
        var all = new HashSet<ProviderCategory>(Enum.GetValues<ProviderCategory>());
        var map = new Dictionary<AuthorityRole, IReadOnlySet<ProviderCategory>>();
        foreach (var role in Enum.GetValues<AuthorityRole>())
            map[role] = all;
        return new AuthorityProviderMatrix(map);
    }
}
