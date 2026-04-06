namespace Whycespace.Engines.T2E.Economic.Capital.Reserve;

public record ReserveCommand(string Action, string EntityId, object Payload);

public sealed record CreateReserveCommand(string Id)
    : ReserveCommand("Create", Id, null!);
