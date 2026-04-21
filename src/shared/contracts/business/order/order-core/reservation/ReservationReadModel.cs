namespace Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;

public sealed record ReservationReadModel
{
    public Guid ReservationId { get; init; }
    public Guid OrderId { get; init; }
    public Guid? LineItemId { get; init; }
    public int SubjectKind { get; init; }
    public Guid SubjectId { get; init; }
    public decimal QuantityValue { get; init; }
    public string QuantityUnit { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset HeldAt { get; init; }
    public DateTimeOffset? ConfirmedAt { get; init; }
    public DateTimeOffset? ReleasedAt { get; init; }
    public DateTimeOffset? ExpiredAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
