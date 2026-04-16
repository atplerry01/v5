namespace Whycespace.Tests.Integration.FailureRecovery;

/// <summary>
/// phase1.5-S5.2.6 / FR-1: xUnit collection that serializes every test
/// class which drives the shared Postgres outbox table. Required because
/// any test that starts a real <see cref="Whycespace.Platform.Host.Adapters.KafkaOutboxPublisher"/>
/// (or replicates its SELECT contract directly) operates on the WHOLE
/// table — production code intentionally does not filter by
/// correlation_id — and would otherwise interfere with concurrent
/// outbox-table tests under xUnit's default class-level parallelism.
///
/// Member classes today:
///   - <see cref="OutboxKafkaOutageRecoveryTest"/> (this folder)
///   - <see cref="Whycespace.Tests.Integration.Platform.Host.Adapters.OutboxMultiInstanceSafetyTest"/>
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class OutboxSharedTableCollection
{
    public const string Name = "OutboxSharedTable";
}
