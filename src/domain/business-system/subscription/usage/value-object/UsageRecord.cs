namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public readonly record struct UsageRecord
{
    public Guid EnrollmentReference { get; }
    public string MetricName { get; }

    public UsageRecord(Guid enrollmentReference, string metricName)
    {
        if (enrollmentReference == Guid.Empty)
            throw new ArgumentException("EnrollmentReference must not be empty.", nameof(enrollmentReference));

        if (string.IsNullOrWhiteSpace(metricName))
            throw new ArgumentException("MetricName must not be empty.", nameof(metricName));

        EnrollmentReference = enrollmentReference;
        MetricName = metricName;
    }
}
