namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public sealed class CanStartSpecification
{
    public bool IsSatisfiedBy(JobStatus status)
    {
        return status == JobStatus.Created;
    }
}
