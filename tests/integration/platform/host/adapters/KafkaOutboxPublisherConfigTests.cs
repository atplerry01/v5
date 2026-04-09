using Confluent.Kafka;
using Npgsql;
using NSubstitute;
using Whyce.Platform.Host.Adapters;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Health;
using Whyce.Shared.Contracts.Infrastructure.Messaging;
using Whyce.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Platform.Host.Adapters;

/// <summary>
/// phase1.6-S1.5 (OUTBOX-CONFIG-01) — pins the constructor contract that
/// externalises MAX_RETRY from <see cref="KafkaOutboxPublisher"/> into
/// <see cref="OutboxOptions"/>.
///
/// phase1.5-S5.2.5 / TB-1: rewritten for the post-PC-4 / HC-5 publisher
/// constructor surface, which now requires
/// <see cref="EventStoreDataSource"/>, <see cref="IWorkerLivenessRegistry"/>
/// and <see cref="IClock"/> in place of the pre-PC-4 raw connection string.
/// The tests themselves do NOT touch Postgres or Kafka — the producer is
/// an NSubstitute stub, the data source is built from a stub connection
/// string that is never opened, and no <c>ExecuteAsync</c> call is made —
/// so they run in milliseconds and need no external infra.
///
/// What is pinned here:
///   1. <see cref="OutboxOptions.MaxRetry"/> default matches the pre-S1.5
///      hardcoded constant (5) so unconfigured deployments stay
///      byte-identical.
///   2. The publisher constructor REQUIRES options, the topic resolver,
///      the worker liveness registry, and the clock (none nullable, none
///      defaulted) and rejects invalid values up-front.
///   3. The record is immutable (init-only), preventing post-construction
///      mutation that would silently desync from the stored field.
/// </summary>
public sealed class KafkaOutboxPublisherConfigTests
{
    private static IProducer<string, string> Producer() =>
        Substitute.For<IProducer<string, string>>();

    private static TopicNameResolver Resolver() => new();

    private static IWorkerLivenessRegistry Liveness() =>
        Substitute.For<IWorkerLivenessRegistry>();

    private static IClock Clock() => new TestClock();

    /// <summary>
    /// Builds a never-opened EventStoreDataSource so the constructor has
    /// the type it needs without requiring a live Postgres. The data
    /// source is created lazily and never has its underlying connection
    /// opened during these tests.
    /// </summary>
    private static EventStoreDataSource DataSource() =>
        new(NpgsqlDataSource.Create("Host=ignored;Database=ignored"));

    [Fact]
    public void OutboxOptions_Default_MaxRetry_Matches_Pre_S1_5_Hardcoded_Constant()
    {
        var options = new OutboxOptions();

        // The pre-S1.5 KafkaOutboxPublisher had `private const int
        // DefaultMaxRetryCount = 5;`. Unconfigured deployments must
        // remain byte-identical, so this default is the load-bearing
        // backwards-compat invariant.
        Assert.Equal(5, options.MaxRetry);
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Null_Options()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new KafkaOutboxPublisher(
                dataSource: DataSource(),
                producer: Producer(),
                topicNameResolver: Resolver(),
                options: null!,
                liveness: Liveness(),
                clock: Clock()));
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Null_TopicNameResolver()
    {
        // phase1.6-S1.6 (DLQ-RESOLVER-01): the resolver is a REQUIRED
        // dependency. Without it the publisher cannot construct a DLQ
        // topic by any path — there is no inline string fallback.
        Assert.Throws<ArgumentNullException>(() =>
            new KafkaOutboxPublisher(
                dataSource: DataSource(),
                producer: Producer(),
                topicNameResolver: null!,
                options: new OutboxOptions(),
                liveness: Liveness(),
                clock: Clock()));
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Zero_MaxRetry()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new KafkaOutboxPublisher(
                dataSource: DataSource(),
                producer: Producer(),
                topicNameResolver: Resolver(),
                options: new OutboxOptions { MaxRetry = 0 },
                liveness: Liveness(),
                clock: Clock()));

        Assert.Contains("at least 1", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Negative_MaxRetry()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new KafkaOutboxPublisher(
                dataSource: DataSource(),
                producer: Producer(),
                topicNameResolver: Resolver(),
                options: new OutboxOptions { MaxRetry = -1 },
                liveness: Liveness(),
                clock: Clock()));
    }

    [Fact]
    public void Publisher_Constructor_Accepts_Custom_MaxRetry()
    {
        var publisher = new KafkaOutboxPublisher(
            dataSource: DataSource(),
            producer: Producer(),
            topicNameResolver: Resolver(),
            options: new OutboxOptions { MaxRetry = 17 },
            liveness: Liveness(),
            clock: Clock());

        Assert.NotNull(publisher);
    }

    [Fact]
    public void OutboxOptions_Supports_Non_Destructive_Mutation_Via_With()
    {
        // Records support `with` for non-destructive update. Verifies the
        // type stays a record (a future refactor that converts to a class
        // with mutable setters would still compile but would change the
        // semantics; this test catches the intent regression).
        var instance = new OutboxOptions { MaxRetry = 3 };
        var copy = instance with { MaxRetry = 9 };

        Assert.Equal(3, instance.MaxRetry);
        Assert.Equal(9, copy.MaxRetry);
    }
}
