using Whycespace.Runtime.EventFabric;

namespace Whycespace.Tests.Integration.Runtime.EventFabric;

/// <summary>
/// phase1.6-S1.6 (DLQ-RESOLVER-01): pin every branch of the canonical
/// dead-letter topic naming convention. The resolver is the single source
/// of truth for DLQ topic names; KafkaOutboxPublisher and any future
/// caller route through it. Inline string manipulation is forbidden by
/// architecture invariant (see WbsmArchitectureTests.No_inline_DLQ_topic_
/// derivation_outside_resolver).
///
/// Lives in the integration test project (alongside the publisher tests)
/// because the unit project does not reference the runtime project. The
/// resolver itself has zero dependencies, so the tests are pure-function
/// assertions that run in microseconds.
/// </summary>
public sealed class TopicNameResolverDeadLetterTests
{
    private static readonly TopicNameResolver Resolver = new();

    [Fact]
    public void Events_Topic_Replaces_Suffix_With_Deadletter()
    {
        var dlq = Resolver.ResolveDeadLetter("whyce.operational.sandbox.todo.events");
        Assert.Equal("whyce.operational.sandbox.todo.deadletter", dlq);
    }

    [Fact]
    public void Already_Deadletter_Topic_Is_Returned_Unchanged_Idempotency()
    {
        // Load-bearing: a recovery loop that re-publishes a row whose
        // source topic was already a DLQ topic must NOT produce
        // x.deadletter.deadletter.
        var input = "whyce.operational.sandbox.todo.deadletter";
        var dlq = Resolver.ResolveDeadLetter(input);
        Assert.Equal(input, dlq);
    }

    [Fact]
    public void Calling_Resolver_Twice_Is_Idempotent()
    {
        // Stronger idempotency check: chained calls produce the same
        // result as a single call.
        var once = Resolver.ResolveDeadLetter("whyce.foo.bar.baz.events");
        var twice = Resolver.ResolveDeadLetter(once);
        Assert.Equal(once, twice);
    }

    [Fact]
    public void Arbitrary_Suffix_Topic_Appends_Deadletter()
    {
        // Defensive case for non-canonical topic names that arrive via
        // outbox rows persisted before topic naming was tightened. The
        // resolver still produces a deterministic DLQ name; topology
        // validation is the resolver.Resolve(envelope, ...) entry
        // point's job, not this method's.
        var dlq = Resolver.ResolveDeadLetter("legacy.topic.name");
        Assert.Equal("legacy.topic.name.deadletter", dlq);
    }

    [Fact]
    public void Null_Topic_Throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Resolver.ResolveDeadLetter(null!));
    }

    [Fact]
    public void Empty_Topic_Throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Resolver.ResolveDeadLetter(string.Empty));
    }

    [Fact]
    public void Whitespace_Topic_Throws_ArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Resolver.ResolveDeadLetter("   "));
    }

    [Fact]
    public void Events_Suffix_Match_Is_Suffix_Only_Not_Substring()
    {
        // A topic that contains ".events" mid-string but does not END
        // with it must NOT have its substring replaced. Catches a naive
        // .Replace(".events", ".deadletter") implementation.
        var dlq = Resolver.ResolveDeadLetter("whyce.events.x.y.commands");
        Assert.Equal("whyce.events.x.y.commands.deadletter", dlq);
    }
}
