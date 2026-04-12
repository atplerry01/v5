namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public sealed class CanCompleteSpecification
{
    public bool IsSatisfiedBy(JobStatus status)
    {
        return status == JobStatus.Running;
    }
}
