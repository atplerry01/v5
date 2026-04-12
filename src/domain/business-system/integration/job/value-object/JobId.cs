namespace Whycespace.Domain.BusinessSystem.Integration.Job;

public readonly record struct JobId
{
    public Guid Value { get; }

    public JobId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("JobId value must not be empty.", nameof(value));
        Value = value;
    }
}
