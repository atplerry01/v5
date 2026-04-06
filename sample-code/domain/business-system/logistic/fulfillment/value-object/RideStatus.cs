namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

/// <summary>
/// Represents the lifecycle status of a RideAggregate.
/// </summary>
public sealed record RideStatus(string Value)
{
    public static readonly RideStatus Requested = new("Requested");
    public static readonly RideStatus Accepted = new("Accepted");
    public static readonly RideStatus InProgress = new("InProgress");
    public static readonly RideStatus Completed = new("Completed");
    public static readonly RideStatus Cancelled = new("Cancelled");

    public bool IsTerminal => this == Completed || this == Cancelled;
}
