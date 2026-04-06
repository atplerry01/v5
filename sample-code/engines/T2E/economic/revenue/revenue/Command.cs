namespace Whycespace.Engines.T2E.Economic.Revenue.Revenue;

public record RevenueCommand(string Action, string EntityId, object Payload);
public sealed record CreateRevenueCommand(string Id, string SettlementId, decimal Amount, string CurrencyCode) : RevenueCommand("Create", Id, null!);
public sealed record RecognizeRevenueCommand(string RevenueId, decimal Amount, string CurrencyCode, string ObligationStatus) : RevenueCommand("Recognize", RevenueId, null!);
public sealed record ReverseRevenueCommand(string RevenueId, decimal Amount, string CurrencyCode) : RevenueCommand("Reverse", RevenueId, null!);
