using Whycespace.Shared.Contracts.Control.Observability.SystemSignal;
using Whycespace.Shared.Contracts.Events.Control.Observability.SystemSignal;

namespace Whycespace.Projections.Control.Observability.SystemSignal.Reducer;

public static class SystemSignalProjectionReducer
{
    public static SystemSignalReadModel Apply(SystemSignalReadModel state, SystemSignalDefinedEventSchema e) =>
        state with
        {
            SignalId     = e.AggregateId,
            Name         = e.Name,
            Kind         = e.Kind,
            Source       = e.Source,
            IsDeprecated = false
        };

    public static SystemSignalReadModel Apply(SystemSignalReadModel state, SystemSignalDeprecatedEventSchema e) =>
        state with { IsDeprecated = true };
}
