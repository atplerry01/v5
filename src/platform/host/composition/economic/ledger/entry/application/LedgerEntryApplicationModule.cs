using Microsoft.Extensions.DependencyInjection;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Ledger.Entry.Application;

/// <summary>
/// Composition module for the economic/ledger/entry domain.
///
/// Entry is a derivative domain: <c>LedgerEntryAggregate</c> instances are
/// created as side effects of journal posting (see <c>PostJournalEntriesHandler</c>),
/// not through a dedicated command surface. <c>LedgerPolicyModule</c> intentionally
/// excludes entry for the same reason.
///
/// This module exists for composition-root symmetry with the other four ledger
/// domains (journal, ledger, obligation, treasury) and as the canonical hook
/// for a future emission path. Today it registers no command handlers.
/// </summary>
public static class LedgerEntryApplicationModule
{
    public static IServiceCollection AddLedgerEntryApplication(this IServiceCollection services)
    {
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
    }
}
