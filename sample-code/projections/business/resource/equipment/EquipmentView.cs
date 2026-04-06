namespace Whycespace.Projections.Business.Resource.Equipment;

public sealed record EquipmentView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
