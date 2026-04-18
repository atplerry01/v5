using Whycespace.Runtime.Middleware.Policy;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Phase8;

/// <summary>
/// Phase 8 B7 — end-to-end validation of the B6 policy middleware input
/// enrichment. Exercises the REAL runtime pipeline (via
/// <see cref="TestHost"/>) with a <see cref="CapturingPolicyEvaluator"/>
/// that records every <see cref="PolicyContext"/> the middleware
/// constructed. Assertions then inspect <c>Calls[0].PolicyContext.*</c>
/// to confirm <c>input.command</c>, <c>input.resource.state</c>,
/// <c>input.now</c>, and <c>input.aggregate_id</c> are populated per
/// the canonical B6 envelope.
///
/// <para>
/// Also validates <see cref="PolicyInputBuilder"/> directly as a pure
/// unit-level predicate for the builder's determinism contract.
/// </para>
/// </summary>
public sealed class PolicyInputEnrichmentTests
{
    // ── Pure builder tests (no middleware pipeline) ──────────────

    [Fact]
    public void Enrich_StampsCommand_ResourceState_Now_AggregateId()
    {
        var baseCtx = new PolicyContext(
            CorrelationId: Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
            TenantId: "tenant-1",
            ActorId: "actor-1",
            CommandType: "TestCommand",
            Roles: new[] { "operator" },
            Classification: "operational",
            Context: "sandbox",
            Domain: "todo");

        var command = new TestCommandWithAggregate(Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"));
        var stateSnapshot = new { status = "Active" };
        var now = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var enriched = PolicyInputBuilder.Enrich(baseCtx, command, stateSnapshot, now);

        Assert.Same(command, enriched.Command);
        Assert.Same(stateSnapshot, enriched.ResourceState);
        Assert.Equal(now, enriched.Now);
        Assert.Equal(command.AggregateId, enriched.AggregateId);

        // Pre-B6 fields preserved verbatim
        Assert.Equal(baseCtx.CorrelationId, enriched.CorrelationId);
        Assert.Equal(baseCtx.CommandType, enriched.CommandType);
    }

    [Fact]
    public void Enrich_NullResourceState_IsPreservedAsExplicitNull()
    {
        var baseCtx = new PolicyContext(
            CorrelationId: Guid.NewGuid(),
            TenantId: "t", ActorId: "a",
            CommandType: "X", Roles: Array.Empty<string>(),
            Classification: "c", Context: "ctx", Domain: "d");

        var enriched = PolicyInputBuilder.Enrich(
            baseCtx,
            command: new TestCommandWithAggregate(Guid.NewGuid()),
            resourceState: null,
            now: DateTimeOffset.UnixEpoch);

        // Null must be stamped, not defaulted — rego's backward-compat
        // branch keys on `not input.resource.state`, which requires the
        // field to be explicitly null.
        Assert.Null(enriched.ResourceState);
    }

    [Fact]
    public void Enrich_CommandWithoutIHasAggregateId_LeavesAggregateIdNull()
    {
        var baseCtx = new PolicyContext(
            CorrelationId: Guid.NewGuid(),
            TenantId: "t", ActorId: "a",
            CommandType: "X", Roles: Array.Empty<string>(),
            Classification: "c", Context: "ctx", Domain: "d");

        var workflowCommand = new TestWorkflowCommandWithoutAggregate();

        var enriched = PolicyInputBuilder.Enrich(
            baseCtx, workflowCommand, resourceState: null, now: DateTimeOffset.UnixEpoch);

        Assert.Null(enriched.AggregateId);
    }

    [Fact]
    public void Enrich_Determinism_IdenticalInputs_ProduceEqualEnrichedContext()
    {
        var baseCtx = new PolicyContext(
            CorrelationId: Guid.Parse("11111111-0000-0000-0000-000000000001"),
            TenantId: "t", ActorId: "a",
            CommandType: "X", Roles: new[] { "r" },
            Classification: "c", Context: "ctx", Domain: "d");

        var cmd = new TestCommandWithAggregate(Guid.Parse("22222222-0000-0000-0000-000000000002"));
        var state = new { status = "Active" };
        var now = new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

        var a = PolicyInputBuilder.Enrich(baseCtx, cmd, state, now);
        var b = PolicyInputBuilder.Enrich(baseCtx, cmd, state, now);

        Assert.Equal(a.Command, b.Command);
        Assert.Equal(a.ResourceState, b.ResourceState);
        Assert.Equal(a.Now, b.Now);
        Assert.Equal(a.AggregateId, b.AggregateId);
    }

    // ── End-to-end through the real middleware pipeline ──────────

    [Fact]
    public async Task Middleware_DispatchesEnrichedInput_CommandPopulated()
    {
        var capturing = new CapturingPolicyEvaluator();
        var host = TestHost.ForTodo(policyEvaluator: capturing);

        var aggregateId = host.IdGenerator.Generate("policy-enrichment:cmd-populated");
        var ctx = host.NewTodoContext(aggregateId);
        var command = new Whycespace.Shared.Contracts.Operational.Sandbox.Todo.CreateTodoCommand(
            aggregateId, "enrichment-test");

        var result = await host.ControlPlane.ExecuteAsync(command, ctx);
        Assert.True(result.IsSuccess, result.Error);

        var call = Assert.Single(capturing.Calls);
        Assert.Same(command, call.PolicyContext.Command);
        Assert.Equal(command.GetType().Name, call.PolicyContext.CommandType);
    }

    [Fact]
    public async Task Middleware_StampsAggregateIdOnPolicyContext_FromIHasAggregateId()
    {
        var capturing = new CapturingPolicyEvaluator();
        var host = TestHost.ForTodo(policyEvaluator: capturing);

        var aggregateId = host.IdGenerator.Generate("policy-enrichment:agg-id");
        var ctx = host.NewTodoContext(aggregateId);
        var command = new Whycespace.Shared.Contracts.Operational.Sandbox.Todo.CreateTodoCommand(
            aggregateId, "agg-id-test");

        _ = await host.ControlPlane.ExecuteAsync(command, ctx);

        var call = Assert.Single(capturing.Calls);
        Assert.Equal(aggregateId, call.PolicyContext.AggregateId);
    }

    [Fact]
    public async Task Middleware_NullResourceState_WhenDefaultLoaderRegistered()
    {
        // TestHost composes the default NullAggregateStateLoader (per the
        // B6 composition registration), so every evaluation stamps state
        // as explicit null until a per-command loader is wired.
        var capturing = new CapturingPolicyEvaluator();
        var host = TestHost.ForTodo(policyEvaluator: capturing);

        var aggregateId = host.IdGenerator.Generate("policy-enrichment:null-state");
        var ctx = host.NewTodoContext(aggregateId);
        var command = new Whycespace.Shared.Contracts.Operational.Sandbox.Todo.CreateTodoCommand(
            aggregateId, "null-state");

        _ = await host.ControlPlane.ExecuteAsync(command, ctx);

        var call = Assert.Single(capturing.Calls);
        Assert.Null(call.PolicyContext.ResourceState);
    }

    [Fact]
    public async Task Middleware_StampsNow_FromIClock()
    {
        var capturing = new CapturingPolicyEvaluator();
        var host = TestHost.ForTodo(policyEvaluator: capturing);

        var aggregateId = host.IdGenerator.Generate("policy-enrichment:now");
        var ctx = host.NewTodoContext(aggregateId);
        var command = new Whycespace.Shared.Contracts.Operational.Sandbox.Todo.CreateTodoCommand(
            aggregateId, "now-stamp");

        _ = await host.ControlPlane.ExecuteAsync(command, ctx);

        var call = Assert.Single(capturing.Calls);
        Assert.NotNull(call.PolicyContext.Now);
        Assert.Equal(host.Clock.UtcNow, call.PolicyContext.Now!.Value);
    }

    [Fact]
    public async Task Middleware_DenyDecision_PropagatesDenyReasonOntoCommandResult()
    {
        var capturing = new CapturingPolicyEvaluator
        {
            NextDecision = new PolicyDecision(
                IsAllowed: false,
                PolicyId: "default",
                DecisionHash: "deny-hash",
                DenialReason: "obligation_counterparty_missing")
        };
        var host = TestHost.ForTodo(policyEvaluator: capturing);

        var aggregateId = host.IdGenerator.Generate("policy-enrichment:deny-reason");
        var ctx = host.NewTodoContext(aggregateId);
        var command = new Whycespace.Shared.Contracts.Operational.Sandbox.Todo.CreateTodoCommand(
            aggregateId, "deny-reason-test");

        var result = await host.ControlPlane.ExecuteAsync(command, ctx);

        Assert.False(result.IsSuccess);
        Assert.Equal("obligation_counterparty_missing", result.PolicyDenyReason);

        // Free-form Error string still embeds the reason for caller UX.
        Assert.NotNull(result.Error);
        Assert.Contains("obligation_counterparty_missing", result.Error!);
    }

    [Fact]
    public async Task Middleware_AllowDecision_LeavesPolicyDenyReasonNull()
    {
        var capturing = new CapturingPolicyEvaluator();  // default = allow
        var host = TestHost.ForTodo(policyEvaluator: capturing);

        var aggregateId = host.IdGenerator.Generate("policy-enrichment:allow-null-reason");
        var ctx = host.NewTodoContext(aggregateId);
        var command = new Whycespace.Shared.Contracts.Operational.Sandbox.Todo.CreateTodoCommand(
            aggregateId, "allow-null-reason");

        var result = await host.ControlPlane.ExecuteAsync(command, ctx);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Null(result.PolicyDenyReason);
    }

    // ── Test-only command types ──────────────────────────────────

    private sealed record TestCommandWithAggregate(Guid AggregateId) : IHasAggregateId;

    private sealed record TestWorkflowCommandWithoutAggregate;
}
