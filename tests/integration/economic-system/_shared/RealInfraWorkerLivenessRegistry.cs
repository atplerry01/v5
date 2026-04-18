using Whycespace.Shared.Contracts.Infrastructure.Health;

namespace Whycespace.Tests.Integration.EconomicSystem.Shared;

/// <summary>
/// phase5-operational-activation + phase6-hardening: no-op worker liveness
/// registry for the real-infra test harness. WorkersHealthCheck is not
/// exercised by the certification suite; the seam exists only because
/// KafkaOutboxPublisher requires the dependency.
/// </summary>
internal sealed class RealInfraWorkerLivenessRegistry : IWorkerLivenessRegistry
{
    public void RecordSuccess(string workerName, DateTimeOffset now) { }

    public IReadOnlyList<WorkerLivenessSnapshot> GetSnapshots(DateTimeOffset now) =>
        Array.Empty<WorkerLivenessSnapshot>();
}
