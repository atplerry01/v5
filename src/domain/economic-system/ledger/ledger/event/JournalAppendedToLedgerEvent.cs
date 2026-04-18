using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

// T1.4 — Totals are carried on the event so the ledger can assert the
// per-journal balance invariant (debits == credits) and the cumulative
// ledger-level balance invariant across all appended journals without
// reaching into journal aggregates. Replayed legacy events deserialize
// with zero totals which remain trivially balanced.
public sealed record JournalAppendedToLedgerEvent(
    LedgerId LedgerId,
    Guid JournalId,
    Amount TotalDebit,
    Amount TotalCredit,
    Timestamp AppendedAt) : DomainEvent;
