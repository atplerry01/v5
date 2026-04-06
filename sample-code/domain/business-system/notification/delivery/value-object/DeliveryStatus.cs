namespace Whycespace.Domain.BusinessSystem.Notification.Delivery;

public sealed record DeliveryStatus(string Value)
{
    public static readonly DeliveryStatus Pending = new("pending");
    public static readonly DeliveryStatus Sent = new("sent");
    public static readonly DeliveryStatus Failed = new("failed");

    public bool IsTerminal => this == Sent || this == Failed;
}
