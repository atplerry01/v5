namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed record MetricsSnapshot(
    ViewerCount Viewers,
    PlaybackCount Playbacks,
    ErrorCount Errors,
    DropCount Drops,
    BitrateMeasurement AverageBitrate,
    LatencyMeasurement AverageLatency);
