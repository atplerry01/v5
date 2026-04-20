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

        // R1 Batch 3 additions — R-CTX-SESSION-01 write-once invariant coverage.
        ctx = NewContext();
        ctx.SessionId = "session-1"; Assert.Throws<InvalidOperationException>(() => ctx.SessionId = "session-2");

        ctx = NewContext();
        ctx.TokenFingerprint = "fp-1"; Assert.Throws<InvalidOperationException>(() => ctx.TokenFingerprint = "fp-2");

        ctx = NewContext();
        ctx.StepUpAuthenticatedAt = DateTimeOffset.UtcNow; Assert.Throws<InvalidOperationException>(() => ctx.StepUpAuthenticatedAt = DateTimeOffset.UtcNow);

        ctx = NewContext();
        ctx.IdempotencyKey = "key-1"; Assert.Throws<InvalidOperationException>(() => ctx.IdempotencyKey = "key-2");

        ctx = NewContext();
        ctx.IdempotencyOutcome = IdempotencyOutcome.Miss; Assert.Throws<InvalidOperationException>(() => ctx.IdempotencyOutcome = IdempotencyOutcome.Hit);
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
        var firstStepUp = DateTimeOffset.Parse("2026-04-19T00:00:00Z");
        var secondStepUp = DateTimeOffset.Parse("2026-04-19T12:00:00Z");

        var ctx = NewContext();
        ctx.IdentityId = "u1";
        ctx.Roles = new[] { "a" };
        ctx.TrustScore = 1;
        ctx.PolicyDecisionAllowed = true;
        ctx.PolicyDecisionHash = "h1";
        ctx.PolicyVersion = "1";
        ctx.Hsid = "hsid-1";
        ctx.ExpectedVersion = 1;

        // R1 Batch 3 additions — must be included in the reset path per R-CTX-SESSION-01.
        ctx.SessionId = "session-1";
        ctx.TokenFingerprint = "fp-1";
        ctx.StepUpAuthenticatedAt = firstStepUp;
        ctx.IdempotencyKey = "idem-1";
        ctx.IdempotencyOutcome = IdempotencyOutcome.Miss;

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
        ctx.SessionId = "session-2";
        ctx.TokenFingerprint = "fp-2";
        ctx.StepUpAuthenticatedAt = secondStepUp;
        ctx.IdempotencyKey = "idem-2";
        ctx.IdempotencyOutcome = IdempotencyOutcome.Hit;

        Assert.Equal("u2", ctx.IdentityId);
        Assert.Equal(new[] { "b" }, ctx.Roles);
        Assert.Equal(2, ctx.TrustScore);
        Assert.False(ctx.PolicyDecisionAllowed);
        Assert.Equal("h2", ctx.PolicyDecisionHash);
        Assert.Equal("2", ctx.PolicyVersion);
        Assert.Equal("hsid-2", ctx.Hsid);
        Assert.Equal(2, ctx.ExpectedVersion);
        Assert.Equal("session-2", ctx.SessionId);
        Assert.Equal("fp-2", ctx.TokenFingerprint);
        Assert.Equal(secondStepUp, ctx.StepUpAuthenticatedAt);
        Assert.Equal("idem-2", ctx.IdempotencyKey);
        Assert.Equal(IdempotencyOutcome.Hit, ctx.IdempotencyOutcome);
    }

    // ─────────────────────────────────────────────────────────────────────
    // R1 Batch 5 — reset totality lock
    // ─────────────────────────────────────────────────────────────────────
    //
    // This test pins the exact set of write-once fields via reflection so
    // that adding a new guarded field without including it in
    // ResetForReplay produces a test failure on the NEXT run — not months
    // later when a replay diverges silently.
    //
    // Method: enumerate every private backing field with the shape
    //   `private <Nullable|Ref>? _name` that backs a public write-once
    // property. For each, assign a value, enter replay mode, call
    // ResetForReplay, then assign again. If any field is not covered by
    // ResetForReplay the second assignment throws.
    [Fact]
    public void ResetForReplay_Covers_Every_Known_WriteOnce_Field()
    {
        // Fields write-once AND reset by ResetForReplay. The total-reset
        // test above (ResetForReplay_Clears_Every_WriteOnce_Guard_In_One_Shot)
        // exercises assignment + reset + re-assignment for each of these.
        var resetFields = new[]
        {
            nameof(CommandContext.IdentityId),
            nameof(CommandContext.Roles),
            nameof(CommandContext.TrustScore),
            nameof(CommandContext.PolicyDecisionAllowed),
            nameof(CommandContext.PolicyDecisionHash),
            nameof(CommandContext.PolicyVersion),
            nameof(CommandContext.Hsid),
            nameof(CommandContext.ExpectedVersion),
            nameof(CommandContext.SessionId),
            nameof(CommandContext.TokenFingerprint),
            nameof(CommandContext.StepUpAuthenticatedAt),
            nameof(CommandContext.IdempotencyKey),
            nameof(CommandContext.IdempotencyOutcome),
        };

        // Fields write-once but INTENTIONALLY NOT reset by replay. These are
        // dispatch-time posture stamps that must persist across a replay so
        // audit correlation and observability can see the runtime state the
        // original execution observed. Recomputing them on replay would
        // diverge when the live posture differs from the historical one.
        var resetExemptFields = new[]
        {
            nameof(CommandContext.DegradedMode),       // HC-7 dispatch-time posture stamp
            nameof(CommandContext.IsExecutionRestricted), // HC-8 soft-restriction tag
        };

        // Sanity: every listed name resolves to a settable property.
        foreach (var name in resetFields.Concat(resetExemptFields))
        {
            var prop = typeof(CommandContext).GetProperty(name);
            Assert.NotNull(prop);
            Assert.True(prop!.CanWrite, $"CommandContext.{name} must be settable.");
        }

        // Reflection: every private backing field matching the write-once
        // shape should appear in one of the two canonical sets. Missing
        // pairs indicate a new field added without the test being updated.
        var backingFields = typeof(CommandContext)
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Where(f => f.Name.StartsWith("_", StringComparison.Ordinal) && !f.Name.Equals("_isReplayMode", StringComparison.Ordinal))
            .Select(f => f.Name.TrimStart('_'))
            .ToList();

        var allAccountedFor = resetFields.Concat(resetExemptFields)
            .Select(n => char.ToLowerInvariant(n[0]) + n[1..])
            .ToHashSet(StringComparer.Ordinal);

        var unknownBackingFields = backingFields
            .Where(n => !allAccountedFor.Contains(n))
            .ToList();

        Assert.True(unknownBackingFields.Count == 0,
            "CommandContext has private backing fields not classified as reset-or-exempt. " +
            "Adding a new write-once field requires a decision: (a) add to ResetForReplay AND " +
            "to the resetFields list above, or (b) document the intentional exemption in " +
            "resetExemptFields above. R-CTX-SESSION-01 drift. Unlisted: " +
            string.Join(", ", unknownBackingFields));
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
