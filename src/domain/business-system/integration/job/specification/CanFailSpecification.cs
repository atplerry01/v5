namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public sealed class CanFailSpecification
{
    public bool IsSatisfiedBy(JobStatus status)
    {
        return status == JobStatus.Running;
    }
}
