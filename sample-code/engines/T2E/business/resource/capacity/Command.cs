namespace Whycespace.Engines.T2E.Business.Resource.Capacity;

public record CapacityCommand(string Action, string EntityId, object Payload);
public sealed record CreateCapacityCommand(string Id) : CapacityCommand("Create", Id, null!);
