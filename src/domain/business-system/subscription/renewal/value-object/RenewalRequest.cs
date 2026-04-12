namespace Whycespace.Domain.BusinessSystem.Subscription.Renewal;

public readonly record struct RenewalRequest
{
    public Guid EnrollmentReference { get; }
    public string TermDescription { get; }

    public RenewalRequest(Guid enrollmentReference, string termDescription)
    {
        if (enrollmentReference == Guid.Empty)
            throw new ArgumentException("EnrollmentReference must not be empty.", nameof(enrollmentReference));

        if (string.IsNullOrWhiteSpace(termDescription))
            throw new ArgumentException("TermDescription must not be empty.", nameof(termDescription));

        EnrollmentReference = enrollmentReference;
        TermDescription = termDescription;
    }
}
