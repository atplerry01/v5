namespace Whycespace.Engines.T2E.Economic.Capital.Capital;

public record CapitalCommand(string Action, string EntityId, object Payload);
public sealed record CreateCapitalCommand(string Id) : CapitalCommand("Create", Id, null!);
public sealed record CommitCapitalCommand(string CapitalAccountId, decimal Amount, string CurrencyCode) : CapitalCommand("Commit", CapitalAccountId, null!);
public sealed record AllocateCapitalCommand(string CapitalAccountId, string AllocationTarget, decimal Amount) : CapitalCommand("Allocate", CapitalAccountId, null!);
