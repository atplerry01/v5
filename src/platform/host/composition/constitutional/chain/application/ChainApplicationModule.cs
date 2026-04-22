using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T2E.Constitutional.Chain.AnchorRecord;
using Whycespace.Engines.T2E.Constitutional.Chain.EvidenceRecord;
using Whycespace.Engines.T2E.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Constitutional.Chain.AnchorRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.EvidenceRecord;
using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Platform.Host.Composition.Constitutional.Chain.Application;

public static class ChainApplicationModule
{
    public static IServiceCollection AddChainApplication(this IServiceCollection services)
    {
        services.AddTransient<RecordAnchorHandler>();
        services.AddTransient<SealAnchorHandler>();
        services.AddTransient<RecordEvidenceHandler>();
        services.AddTransient<ArchiveEvidenceHandler>();
        services.AddTransient<OpenLedgerHandler>();
        services.AddTransient<SealLedgerHandler>();
        return services;
    }

    public static void RegisterEngines(IEngineRegistry engine)
    {
        engine.Register<RecordAnchorCommand, RecordAnchorHandler>();
        engine.Register<SealAnchorCommand, SealAnchorHandler>();
        engine.Register<RecordEvidenceCommand, RecordEvidenceHandler>();
        engine.Register<ArchiveEvidenceCommand, ArchiveEvidenceHandler>();
        engine.Register<OpenLedgerCommand, OpenLedgerHandler>();
        engine.Register<SealLedgerCommand, SealLedgerHandler>();
    }
}
