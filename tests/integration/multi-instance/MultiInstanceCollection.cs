namespace Whycespace.Tests.Integration.MultiInstance;

/// <summary>
/// phase1.5-S5.5 / Stage B: serialization collection for multi-instance
/// scenarios. Both Stage B scenarios drive the SHARED `whyce-host-1` /
/// `whyce-host-2` topology and the SHARED Postgres event store + Kafka
/// broker, so they cannot interleave with each other or with anything
/// else that touches the same infrastructure.
///
/// Different from <c>OutboxSharedTableCollection</c> (which serializes
/// in-process tests against the local Postgres outbox table) — this
/// one serializes tests against the live Docker compose multi-instance
/// stack on host ports 18080/18081/18082.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MultiInstanceCollection
{
    public const string Name = "MultiInstance";
}
