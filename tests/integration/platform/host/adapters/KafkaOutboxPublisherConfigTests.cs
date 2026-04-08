using Confluent.Kafka;
using NSubstitute;
using Whyce.Platform.Host.Adapters;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Infrastructure.Messaging;

namespace Whycespace.Tests.Integration.Platform.Host.Adapters;

/// <summary>
/// phase1.6-S1.5 (OUTBOX-CONFIG-01): pin the constructor contract that
/// externalises MAX_RETRY from <see cref="KafkaOutboxPublisher"/> into
/// <see cref="OutboxOptions"/>.
///
/// Lives in the integration test project (not unit) because the publisher
/// is in <c>Whyce.Platform.Host</c> and the unit project intentionally
/// does not reference Host. The tests themselves do NOT touch Postgres or
/// Kafka — the producer is an NSubstitute stub and no ExecuteAsync call
/// is made — so they run in milliseconds and need no external infra.
///
/// Why these tests are constructor-focused: the publisher is a
/// BackgroundService that drives a real Postgres outbox + Kafka producer,
/// so end-to-end retry-vs-DLQ exercise belongs against live infra. What
/// CAN be pinned in test scope, and what is the regression-prone surface,
/// is:
///   1. OutboxOptions.MaxRetry default matches the pre-S1.5 hardcoded
///      constant (5) so unconfigured deployments stay byte-identical.
///   2. The publisher constructor REQUIRES options (not nullable, not
///      defaulted) and rejects invalid values up-front.
///   3. The record is immutable (init-only), preventing post-construction
///      mutation that would silently desync from the stored field.
/// </summary>
public sealed class KafkaOutboxPublisherConfigTests
{
    private static IProducer<string, string> Producer() =>
        Substitute.For<IProducer<string, string>>();

    // phase1.6-S1.6: TopicNameResolver is a sealed concrete class with no
    // dependencies, so a real instance is the simplest test seed. There
    // is no behavior to mock — every method is a deterministic pure
    // function over its inputs.
    private static TopicNameResolver Resolver() => new();

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
                connectionString: "Host=ignored",
                producer: Producer(),
                topicNameResolver: Resolver(),
                options: null!));
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Null_TopicNameResolver()
    {
        // phase1.6-S1.6 (DLQ-RESOLVER-01): the resolver is a REQUIRED
        // dependency. Without it the publisher cannot construct a DLQ
        // topic by any path — there is no inline string fallback.
        Assert.Throws<ArgumentNullException>(() =>
            new KafkaOutboxPublisher(
                connectionString: "Host=ignored",
                producer: Producer(),
                topicNameResolver: null!,
                options: new OutboxOptions()));
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Zero_MaxRetry()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new KafkaOutboxPublisher(
                connectionString: "Host=ignored",
                producer: Producer(),
                topicNameResolver: Resolver(),
                options: new OutboxOptions { MaxRetry = 0 }));

        Assert.Contains("at least 1", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Publisher_Constructor_Rejects_Negative_MaxRetry()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new KafkaOutboxPublisher(
                connectionString: "Host=ignored",
                producer: Producer(),
                topicNameResolver: Resolver(),
                options: new OutboxOptions { MaxRetry = -1 }));
    }

    [Fact]
    public void Publisher_Constructor_Accepts_Custom_MaxRetry()
    {
        var publisher = new KafkaOutboxPublisher(
            connectionString: "Host=ignored",
            producer: Producer(),
            topicNameResolver: Resolver(),
            options: new OutboxOptions { MaxRetry = 17 });

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
