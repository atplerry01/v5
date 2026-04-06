namespace Whycespace.Shared.Contracts.Domain.Economic;

/// <summary>
/// Economic context for policy decision binding and chain anchoring (E6).
/// Resolved from command payload by the runtime.
/// Carried through the pipeline to enrich policy decisions and chain evidence.
/// </summary>
public interface IEconomicContext
{
    string AccountId { get; }
    string AssetId { get; }
    decimal Amount { get; }
    string Currency { get; }
    string TransactionType { get; }
}
