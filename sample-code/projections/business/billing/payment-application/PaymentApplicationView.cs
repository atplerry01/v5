namespace Whycespace.Projections.Business.Billing.PaymentApplication;

public sealed record PaymentApplicationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
