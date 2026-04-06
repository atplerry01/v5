namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class SpvOperatorSpec
{
    public bool IsSatisfiedBy(SpvAggregate spv)
    {
        return spv.Status == SpvStatus.Active
            && spv.Operators.Count > 0;
    }
}
