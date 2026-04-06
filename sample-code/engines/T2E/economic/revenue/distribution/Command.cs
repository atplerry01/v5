namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

public record DistributionCommand(string Action, string EntityId, object Payload);
public sealed record CreateDistributionCommand(string Id, Guid RevenueId, decimal Amount, string CurrencyCode) : DistributionCommand("Create", Id, null!);
