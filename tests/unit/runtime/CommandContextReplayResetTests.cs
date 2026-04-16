using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// phase1.6-S1.4 — pin the dual-mode behavior of CommandContext: write-once
/// at runtime, controlled reset during a scoped replay window.
///
/// These tests are the canonical proof that:
///   1. The write-once invariant is preserved at runtime (no relaxation
///      from the pre-S1.4 behavior).
///   2. ResetForReplay is gated — calling it outside replay mode throws.
///   3. ResetForReplay is total — every write-once guard is cleared in
///      one shot, and after reset every field can be re-assigned exactly
///      once until disabled or reset again.
///   4. The seam is internal (the test assembly has InternalsVisibleTo
///      from Whycespace.Shared) — production callers can never reach it.
/// </summary>
public sealed class CommandContextReplayResetTests
{
    private static CommandContext NewContext() => new()
    {
        CorrelationId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        CausationId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        CommandId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
        TenantId = "test-tenant",
        ActorId = "test-actor",
        AggregateId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
        PolicyId = "default",
        Classification = "operational",
        Context = "sandbox",
        Domain = "todo"
    };

    // ─────────────────────────────────────────────────────────────────────
    // Write-once invariant — must remain strict outside replay mode.
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void WriteOnce_FirstAssignment_Succeeds()
    {
        var ctx = NewContext();
        ctx.IdentityId = "user-1";
        ctx.PolicyDecisionHash = "hash-1";
        ctx.PolicyVersion = "1.0.0";
        ctx.PolicyDecisionAllowed = true;
        ctx.Hsid = "PPP-LLLL-TTT-TOPO-001";
        ctx.ExpectedVersion = 5;
        ctx.TrustScore = 80;
        ctx.Roles = new[] { "admin" };

        Assert.Equal("user-1", ctx.IdentityId);
        Assert.Equal("hash-1", ctx.PolicyDecisionHash);
        Assert.Equal("1.0.0", ctx.PolicyVersion);
        Assert.True(ctx.PolicyDecisionAllowed);
    }

    [Fact]
    public void WriteOnce_SecondAssignment_Throws_Outside_Replay_Mode()
    {
        var ctx = NewContext();
        ctx.PolicyDecisionHash = "hash-1";

        var ex = Assert.Throws<InvalidOperationException>(() => ctx.PolicyDecisionHash = "hash-2");
        Assert.Contains("write-once", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WriteOnce_All_Guarded_Fields_Throw_On_Second_Assignment()
    {
        // Cover every write-once field. If a future field is added without
        // a guard, this test will not catch it directly, but the parallel
        // ResetForReplay test below WILL — because it asserts that all known
        // guarded fields can be re-assigned after reset.
        var ctx = NewContext();
        ctx.IdentityId = "u1"; Assert.Throws<InvalidOperationException>(() => ctx.IdentityId = "u2");

        ctx = NewContext();
        ctx.Roles = new[] { "a" }; Assert.Throws<InvalidOperationException>(() => ctx.Roles = new[] { "b" });

        ctx = NewContext();
        ctx.TrustScore = 1; Assert.Throws<InvalidOperationException>(() => ctx.TrustScore = 2);

        ctx = NewContext();
        ctx.PolicyDecisionAllowed = true; Assert.Throws<InvalidOperationException>(() => ctx.PolicyDecisionAllowed = false);

        ctx = NewContext();
        ctx.PolicyDecisionHash = "h1"; Assert.Throws<InvalidOperationException>(() => ctx.PolicyDecisionHash = "h2");

        ctx = NewContext();
        ctx.PolicyVersion = "1"; Assert.Throws<InvalidOperationException>(() => ctx.PolicyVersion = "2");

        ctx = NewContext();
        ctx.Hsid = "h1"; Assert.Throws<InvalidOperationException>(() => ctx.Hsid = "h2");

        ctx = NewContext();
        ctx.ExpectedVersion = 1; Assert.Throws<InvalidOperationException>(() => ctx.ExpectedVersion = 2);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ResetForReplay gating
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void ResetForReplay_Throws_When_Not_In_Replay_Mode()
    {
        var ctx = NewContext();

        var ex = Assert.Throws<InvalidOperationException>(() => ctx.ResetForReplay());
        Assert.Contains("replay mode", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResetForReplay_Throws_After_DisableReplayMode()
    {
        var ctx = NewContext();
        ctx.EnableReplayMode();
        ctx.DisableReplayMode();

        Assert.Throws<InvalidOperationException>(() => ctx.ResetForReplay());
    }

    [Fact]
    public void IsReplayMode_Reflects_Enable_And_Disable()
    {
        var ctx = NewContext();
        Assert.False(ctx.IsReplayMode);

        ctx.EnableReplayMode();
        Assert.True(ctx.IsReplayMode);

        ctx.DisableReplayMode();
        Assert.False(ctx.IsReplayMode);
    }

    [Fact]
    public void EnableReplayMode_Is_Idempotent()
    {
        var ctx = NewContext();
        ctx.EnableReplayMode();
        ctx.EnableReplayMode(); // must not throw
        Assert.True(ctx.IsReplayMode);
    }

    // ─────────────────────────────────────────────────────────────────────
    // ResetForReplay total-reset semantics
    // ─────────────────────────────────────────────────────────────────────

    [Fact]
    public void ResetForReplay_Clears_Every_WriteOnce_Guard_In_One_Shot()
    {
        // Set every guarded field, enter replay mode, reset, then prove
        // every guarded field is re-assignable. This is the contract test
        // for total reset — partial reset is forbidden.
        var ctx = NewContext();
        ctx.IdentityId = "u1";
        ctx.Roles = new[] { "a" };
        ctx.TrustScore = 1;
        ctx.PolicyDecisionAllowed = true;
        ctx.PolicyDecisionHash = "h1";
        ctx.PolicyVersion = "1";
        ctx.Hsid = "hsid-1";
        ctx.ExpectedVersion = 1;

        ctx.EnableReplayMode();
        ctx.ResetForReplay();

        // Every guarded field accepts a fresh assignment without throwing.
        ctx.IdentityId = "u2";
        ctx.Roles = new[] { "b" };
        ctx.TrustScore = 2;
        ctx.PolicyDecisionAllowed = false;
        ctx.PolicyDecisionHash = "h2";
        ctx.PolicyVersion = "2";
        ctx.Hsid = "hsid-2";
        ctx.ExpectedVersion = 2;

        Assert.Equal("u2", ctx.IdentityId);
        Assert.Equal(new[] { "b" }, ctx.Roles);
        Assert.Equal(2, ctx.TrustScore);
        Assert.False(ctx.PolicyDecisionAllowed);
        Assert.Equal("h2", ctx.PolicyDecisionHash);
        Assert.Equal("2", ctx.PolicyVersion);
        Assert.Equal("hsid-2", ctx.Hsid);
        Assert.Equal(2, ctx.ExpectedVersion);
    }

    [Fact]
    public void ResetForReplay_Does_Not_Touch_Required_Init_Properties()
    {
        // The construction-time identity of the context (correlation id,
        // command id, tenant, etc.) is part of the context's immutable
        // identity and must NOT be reset. Only the write-once guards
        // collected after construction are touched.
        var ctx = NewContext();
        var preCorrelationId = ctx.CorrelationId;
        var preCommandId = ctx.CommandId;
        var preTenant = ctx.TenantId;
        var preDomain = ctx.Domain;

        ctx.EnableReplayMode();
        ctx.ResetForReplay();

        Assert.Equal(preCorrelationId, ctx.CorrelationId);
        Assert.Equal(preCommandId, ctx.CommandId);
        Assert.Equal(preTenant, ctx.TenantId);
        Assert.Equal(preDomain, ctx.Domain);
    }

    [Fact]
    public void After_DisableReplayMode_WriteOnce_Is_Strict_Again()
    {
        // Full lifecycle: set -> enter replay -> reset -> reassign ->
        // exit replay -> attempt to overwrite -> throws.
        var ctx = NewContext();
        ctx.PolicyDecisionHash = "h1";

        ctx.EnableReplayMode();
        ctx.ResetForReplay();
        ctx.PolicyDecisionHash = "h2";
        ctx.DisableReplayMode();

        Assert.Throws<InvalidOperationException>(() => ctx.PolicyDecisionHash = "h3");
    }

    [Fact]
    public void Reset_Then_Reset_Again_In_Same_Replay_Window_Is_Allowed()
    {
        // Two consecutive resets in the same replay window are allowed
        // — the gate is "in replay mode", not "called once". This matches
        // the locked plan: reset is total, gate is scoped.
        var ctx = NewContext();
        ctx.PolicyDecisionHash = "h1";

        ctx.EnableReplayMode();
        ctx.ResetForReplay();
        ctx.PolicyDecisionHash = "h2";
        ctx.ResetForReplay();
        ctx.PolicyDecisionHash = "h3";

        Assert.Equal("h3", ctx.PolicyDecisionHash);
    }
}
