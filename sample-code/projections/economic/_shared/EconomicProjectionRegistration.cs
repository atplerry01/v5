using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Projections.Economic;

/// <summary>
/// Builds projection registrations for all economic projections.
/// Each handler uses IdempotentEconomicProjectionHandler for EventId-based deduplication.
/// </summary>
public static class EconomicProjectionRegistration
{
    public static IReadOnlyList<ProjectionRegistration> BuildRegistrations(EconomicReadStore readStore, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(readStore);
        ArgumentNullException.ThrowIfNull(clock);

        return
        [
            new WalletBalanceProjectionHandler(readStore, clock).ToRegistration(),
            new TransactionHistoryProjectionHandler(readStore, clock).ToRegistration(),
            new EnforcementProjectionHandler(readStore, clock).ToRegistration(),
            new LimitUsageProjectionHandler(readStore, clock).ToRegistration(),
        ];
    }

    public static void RegisterAll(IProjectionEngine engine, EconomicReadStore readStore, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(engine);

        foreach (var registration in BuildRegistrations(readStore, clock))
            engine.Register(registration);
    }
}
