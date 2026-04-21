using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

namespace Whycespace.Domain.StructuralSystem.Structure.RelationshipRules;

public sealed class AuthoritySubclusterConstraint
{
    public IReadOnlySet<AuthorityRole> PermittedRoles { get; }

    public AuthoritySubclusterConstraint(IReadOnlySet<AuthorityRole> permittedRoles)
    {
        if (permittedRoles is null)
            throw new ArgumentNullException(nameof(permittedRoles));

        PermittedRoles = permittedRoles;
    }

    public bool IsAllowed(AuthorityRole role) => PermittedRoles.Contains(role);

    public static AuthoritySubclusterConstraint Permissive { get; } =
        new(new HashSet<AuthorityRole>(Enum.GetValues<AuthorityRole>()));
}
