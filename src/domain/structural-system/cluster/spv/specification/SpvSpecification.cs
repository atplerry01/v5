namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Created;
    }
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Active;
    }
}

public sealed class CanCloseSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Suspended;
    }
}
