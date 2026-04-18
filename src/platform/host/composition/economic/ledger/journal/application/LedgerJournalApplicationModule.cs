using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Economic.Ledger.Journal.Application;

public static class LedgerJournalApplicationModule
{
    public static IServiceCollection AddLedgerJournalApplication(this IServiceCollection services)
    {
        services.AddTransient<PostJournalEntriesHandler>();
        services.AddTransient<PostCompensatingJournalHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<PostJournalEntriesCommand, PostJournalEntriesHandler>();
        engine.Register<PostCompensatingJournalCommand, PostCompensatingJournalHandler>();
    }
}
