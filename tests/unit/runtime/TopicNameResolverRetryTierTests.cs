using Whycespace.Runtime.EventFabric;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// R2.A.3a — pin the tiered topic resolution shape (R-TOPIC-TIER-01).
/// Canonical order: <c>.events</c> → <c>.retry</c> → <c>.deadletter</c>.
/// Covers every branch of <see cref="TopicNameResolver.ResolveRetry"/> and
/// the extended <see cref="TopicNameResolver.ResolveDeadLetter"/>
/// <c>.retry</c> → <c>.deadletter</c> transition.
/// </summary>
public sealed class TopicNameResolverRetryTierTests
{
    private readonly TopicNameResolver _resolver = new();

    // ─────────────────────────────────────────────────────────────────────
    // ResolveRetry — happy path + idempotency
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Resolve_Retry_From_Events_Topic_Replaces_Suffix()
    {
        var result = _resolver.ResolveRetry("whyce.economic.ledger.journal.events");
        Assert.Equal("whyce.economic.ledger.journal.retry", result);
    }

    [Fact]
    public void Resolve_Retry_Is_Idempotent_On_Retry_Topic()
    {
        var result = _resolver.ResolveRetry("whyce.economic.ledger.journal.retry");
        Assert.Equal("whyce.economic.ledger.journal.retry", result);
    }

    [Fact]
    public void Resolve_Retry_Appends_Suffix_For_Unknown_Tier()
    {
        // Fall-through branch — matches ResolveDeadLetter's behavior for
        // callers that pass a non-canonical topic shape (test fixtures, etc.)
        var result = _resolver.ResolveRetry("legacy-topic");
        Assert.Equal("legacy-topic.retry", result);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ResolveRetry — forbidden transitions
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Resolve_Retry_From_Deadletter_Throws()
    {
        // Moving backwards from the terminal tier is forbidden — re-drive
        // is an operator-authorized action, not an automatic fabric transition.
        var ex = Assert.Throws<InvalidTopicException>(
            () => _resolver.ResolveRetry("whyce.economic.ledger.journal.deadletter"));
        Assert.Contains("terminal", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_Retry_From_Commands_Throws()
    {
        // Command-tier retry is IRetryExecutor's responsibility at the
        // call site, not a fabric-tier topic transition.
        var ex = Assert.Throws<InvalidTopicException>(
            () => _resolver.ResolveRetry("whyce.operational.sandbox.todo.commands"));
        Assert.Contains("IRetryExecutor", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_Retry_Rejects_Invalid_Input(string? topic)
    {
        Assert.Throws<ArgumentException>(() => _resolver.ResolveRetry(topic!));
    }

    // ─────────────────────────────────────────────────────────────────────
    // ResolveDeadLetter — NEW: retry → deadletter transition
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Resolve_DeadLetter_From_Retry_Topic_Replaces_Suffix()
    {
        var result = _resolver.ResolveDeadLetter("whyce.economic.ledger.journal.retry");
        Assert.Equal("whyce.economic.ledger.journal.deadletter", result);
    }

    [Fact]
    public void Resolve_DeadLetter_Chain_Events_To_Retry_To_Deadletter_Is_Canonical()
    {
        // Full three-tier chain in one assertion — the locked D2 ordering.
        var events = "whyce.economic.ledger.journal.events";
        var retry = _resolver.ResolveRetry(events);
        var deadletter = _resolver.ResolveDeadLetter(retry);

        Assert.Equal("whyce.economic.ledger.journal.retry", retry);
        Assert.Equal("whyce.economic.ledger.journal.deadletter", deadletter);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ResolveDeadLetter — pre-existing branches still pass
    // (regression guard — R2.A.3a must not change pre-R2.A.3a behavior)
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void Resolve_DeadLetter_Idempotent_On_Deadletter()
    {
        Assert.Equal(
            "whyce.x.y.z.deadletter",
            _resolver.ResolveDeadLetter("whyce.x.y.z.deadletter"));
    }

    [Fact]
    public void Resolve_DeadLetter_From_Events_Topic_Replaces_Suffix()
    {
        Assert.Equal(
            "whyce.x.y.z.deadletter",
            _resolver.ResolveDeadLetter("whyce.x.y.z.events"));
    }

    [Fact]
    public void Resolve_DeadLetter_Falls_Through_For_Unknown_Suffix()
    {
        Assert.Equal(
            "legacy-topic.deadletter",
            _resolver.ResolveDeadLetter("legacy-topic"));
    }
}
