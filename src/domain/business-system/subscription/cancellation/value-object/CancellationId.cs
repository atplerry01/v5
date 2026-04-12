namespace Whycespace.Domain.BusinessSystem.Subscription.Cancellation;

public readonly record struct CancellationId
{
    public Guid Value { get; }

    public CancellationId(Guid value)
    {
        if (value == Guid.Empty)
            throw CancellationErrors.MissingId();

        Value = value;
    }
}

public enum CancellationStatus
{
    Requested,
    Confirmed
}

public readonly record struct CancellationRequest
{
    public Guid EnrollmentReference { get; }
    public string Reason { get; }

    public CancellationRequest(Guid enrollmentReference, string reason)
    {
        if (enrollmentReference == Guid.Empty)
            throw CancellationErrors.MissingRequest();

        if (string.IsNullOrWhiteSpace(reason))
            throw CancellationErrors.MissingRequest();

        EnrollmentReference = enrollmentReference;
        Reason = reason;
    }
}
