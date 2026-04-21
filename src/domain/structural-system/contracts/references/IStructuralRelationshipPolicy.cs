using Whycespace.Domain.StructuralSystem.Structure.RelationshipRules;

namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public interface IStructuralRelationshipPolicy
{
    AuthorityProviderMatrix AuthorityProviderMatrix { get; }
    AuthoritySubclusterConstraint AuthoritySubclusterConstraint { get; }
    SpvScopeConstraint SpvScopeConstraint { get; }
}
