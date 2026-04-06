using Whycespace.Shared.Contracts.Domain.Economic;
using Whycespace.Shared.Contracts.Domain.Economic.Ledger;

namespace Whycespace.Runtime.Economic.Mapping;

/// <summary>
/// Maps graph-aware economic execution context (E17.5) to ledger entry DTOs
/// compatible with the existing ILedgerDomainService contract.
/// Each hop in the execution path produces a balanced debit/credit pair.
/// </summary>
public static class GraphToLedgerMapper
{
    public static IReadOnlyList<LedgerEntryDto> Map(IEconomicGraphContext context, Guid transactionId)
    {
        var nodes = context.ExecutionPath.ToList();
        var entries = new List<LedgerEntryDto>();

        for (var i = 0; i < nodes.Count - 1; i++)
        {
            var from = nodes[i];
            var to = nodes[i + 1];

            entries.Add(new LedgerEntryDto(from, context.Amount, "Debit", transactionId));
            entries.Add(new LedgerEntryDto(to, context.Amount, "Credit", transactionId));
        }

        return entries;
    }
}
