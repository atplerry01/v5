using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Economic.Ledger;

/// <summary>
/// E5 — ledger-context policy bindings. Registers one
/// <see cref="CommandPolicyBinding"/> per ledger-group command, mapping each
/// command CLR type to its canonical policy id constant declared on the
/// matching <c>*PolicyIds</c> class. Aggregated by
/// <see cref="ICommandPolicyIdRegistry"/> at runtime composition so that every
/// dispatch of a ledger command stamps the correct policy id onto
/// <c>CommandContext.PolicyId</c> for evaluation by <c>PolicyMiddleware</c>.
///
/// Coverage: journal (1) + ledger (1) + obligation (3) + treasury (3) = 8 bindings.
/// Entry is intentionally excluded — entries are created only through journal
/// posting, so no standalone command surface exists.
/// </summary>
public static class LedgerPolicyModule
{
    public static IServiceCollection AddLedgerPolicyBindings(this IServiceCollection services)
    {
        // ── journal (1) ───────────────────────────────────────────
        // PostJournalEntries is dispatched by the transaction lifecycle
        // workflow via ISystemIntentDispatcher and MUST have a policy
        // binding — it flows through the full runtime pipeline.
        services.AddSingleton(new CommandPolicyBinding(typeof(PostJournalEntriesCommand), JournalPolicyIds.PostEntries));

        // ── ledger (1) ────────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(OpenLedgerCommand), LedgerPolicyIds.Open));

        // ── obligation (3) ────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateObligationCommand), ObligationPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(FulfilObligationCommand), ObligationPolicyIds.Fulfil));
        services.AddSingleton(new CommandPolicyBinding(typeof(CancelObligationCommand), ObligationPolicyIds.Cancel));

        // ── treasury (3) ──────────────────────────────────────────
        services.AddSingleton(new CommandPolicyBinding(typeof(CreateTreasuryCommand), TreasuryPolicyIds.Create));
        services.AddSingleton(new CommandPolicyBinding(typeof(AllocateFundsCommand), TreasuryPolicyIds.AllocateFunds));
        services.AddSingleton(new CommandPolicyBinding(typeof(ReleaseFundsCommand), TreasuryPolicyIds.ReleaseFunds));

        return services;
    }
}
