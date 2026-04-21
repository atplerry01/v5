namespace Whycespace.Domain.StructuralSystem.Contracts.References;

public interface IStructuralParentLookup
{
    StructuralParentState GetState(ClusterRef parent);
    StructuralParentState GetState(ClusterAuthorityRef parent);
}
