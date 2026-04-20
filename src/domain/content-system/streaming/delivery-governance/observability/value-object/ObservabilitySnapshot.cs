namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed record ObservabilitySnapshot(
    ViewerCount Viewers,
    PlaybackCount Playbacks,
    ErrorCount Errors,
    DropCount Drops,
    BitrateMeasurement AverageBitrate,
    LatencyMeasurement AverageLatency);
