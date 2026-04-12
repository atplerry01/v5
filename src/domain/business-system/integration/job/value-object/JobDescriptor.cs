namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public readonly record struct JobDescriptor
{
    public string JobName { get; }
    public string JobType { get; }

    public JobDescriptor(string jobName, string jobType)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("JobName must not be empty.", nameof(jobName));
        if (string.IsNullOrWhiteSpace(jobType))
            throw new ArgumentException("JobType must not be empty.", nameof(jobType));
        JobName = jobName;
        JobType = jobType;
    }
}
