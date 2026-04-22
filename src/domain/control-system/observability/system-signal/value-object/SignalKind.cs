namespace Whycespace.Domain.ControlSystem.Observability.SystemSignal;

public enum SignalKind
{
    Heartbeat = 1,
    Threshold = 2,
    Anomaly = 3,
    Recovery = 4,
    Degradation = 5
}
