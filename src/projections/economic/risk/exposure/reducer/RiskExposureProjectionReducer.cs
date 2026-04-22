using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Events.Economic.Risk.Exposure;

namespace Whycespace.Projections.Economic.Risk.Exposure.Reducer;

public static class RiskExposureProjectionReducer
{
    // Status codes mirror Whycespace.Domain.DecisionSystem.Risk.Exposure.ExposureStatus.
    // Active=0, Reduced=1, Closed=2.
    private const int StatusActive  = 0;
    private const int StatusReduced = 1;
    private const int StatusClosed  = 2;

    public static RiskExposureReadModel Apply(RiskExposureReadModel state, RiskExposureCreatedEventSchema e) =>
        state with
        {
            ExposureId = e.AggregateId,
            SourceId = e.SourceId,
            ExposureType = e.ExposureType,
            TotalExposure = e.TotalExposure,
            Currency = e.Currency,
            Status = StatusActive,
            CreatedAt = e.CreatedAt
        };

    public static RiskExposureReadModel Apply(RiskExposureReadModel state, RiskExposureIncreasedEventSchema e) =>
        state with
        {
            ExposureId = e.AggregateId,
            TotalExposure = e.NewTotal,
            Status = StatusActive
        };

    public static RiskExposureReadModel Apply(RiskExposureReadModel state, RiskExposureReducedEventSchema e) =>
        state with
        {
            ExposureId = e.AggregateId,
            TotalExposure = e.NewTotal,
            Status = StatusReduced
        };

    public static RiskExposureReadModel Apply(RiskExposureReadModel state, RiskExposureClosedEventSchema e) =>
        state with
        {
            ExposureId = e.AggregateId,
            TotalExposure = 0m,
            Status = StatusClosed
        };
}
