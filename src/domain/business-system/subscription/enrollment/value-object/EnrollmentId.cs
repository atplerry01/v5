namespace Whycespace.Domain.BusinessSystem.Subscription.Enrollment;

public readonly record struct EnrollmentId
{
    public Guid Value { get; }

    public EnrollmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw EnrollmentErrors.MissingId();

        Value = value;
    }
}

public enum EnrollmentStatus
{
    Pending,
    Active,
    Cancelled
}

public readonly record struct EnrollmentRequest
{
    public Guid AccountReference { get; }
    public Guid PlanReference { get; }

    public EnrollmentRequest(Guid accountReference, Guid planReference)
    {
        if (accountReference == Guid.Empty)
            throw EnrollmentErrors.MissingRequest();

        if (planReference == Guid.Empty)
            throw EnrollmentErrors.MissingRequest();

        AccountReference = accountReference;
        PlanReference = planReference;
    }
}
