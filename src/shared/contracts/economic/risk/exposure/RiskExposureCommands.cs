using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Risk.Exposure;

public sealed record CreateRiskExposureCommand(
    Guid ExposureId,
    Guid SourceId,
    int ExposureType,
    decimal InitialExposure,
    string Currency,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

public sealed record IncreaseRiskExposureCommand(
    Guid ExposureId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

public sealed record ReduceRiskExposureCommand(
    Guid ExposureId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}

public sealed record CloseRiskExposureCommand(
    Guid ExposureId) : IHasAggregateId
{
    public Guid AggregateId => ExposureId;
}
