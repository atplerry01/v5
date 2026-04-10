using Whyce.Shared.Contracts.Application.Todo;
using Whycespace.Domain.ConstitutionalSystem.Policy.Decision;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.ConstitutionalSystem.Policy.Decision;

/// <summary>
/// Verifies the WBSM v3 policy eventification contract introduced 2026-04-07
/// (Option-4 ALLOW-path scope). Drives the real RuntimeControlPlane and
/// asserts that PolicyMiddleware emits a PolicyEvaluatedEvent that flows
/// through the event fabric (persist → chain → outbox) alongside the
/// command's domain events.
///
/// Out of scope (deferred per claude/new-rules/20260407-190000-policy-eventification.md):
/// - DENY-path emission (PolicyDeniedEvent)
/// - Dedicated policy topic
/// - Dedicated policy aggregate / audit stream
/// </summary>
public sealed class PolicyEventificationTests
{
    [Fact]
    public async Task Allow_Emits_PolicyEvaluatedEvent_To_Dedicated_Audit_Stream()
    {
        var host = TestHost.ForTodo();
        var aggregateId = host.IdGenerator.Generate("policy:allow:1");
        var context = host.NewTodoContext(aggregateId);

        var result = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Eventify"),
            context);

        Assert.True(result.IsSuccess, result.Error);

        // Policy event lives in its OWN aggregate stream, derived deterministically
        // from CommandId. The Todo aggregate stream contains only domain events.
        var auditStreamId = host.IdGenerator.Generate($"policy-audit-stream:{context.CommandId}");
        var auditStream = host.EventStore.AllEvents(auditStreamId);
        var policyEvent = Assert.IsType<PolicyEvaluatedEvent>(Assert.Single(auditStream));

        // DecisionHash matches the hash captured on CommandContext by the engine.
        Assert.False(string.IsNullOrEmpty(policyEvent.DecisionHash));
        Assert.Equal(context.PolicyDecisionHash, policyEvent.DecisionHash);
        Assert.Equal(context.CorrelationId, policyEvent.CorrelationId);
        Assert.Equal(context.CausationId, policyEvent.CausationId);
        Assert.True(policyEvent.IsAllowed);

        // Two chain blocks + outbox batches: audit batch first (policy decision),
        // then domain batch (todo event). Audit batch publishes to the dedicated topic.
        Assert.Equal(2, host.ChainAnchor.Blocks.Count);
        Assert.Equal(2, host.Outbox.Batches.Count);
        Assert.Equal("whyce.constitutional.policy.decision.events", host.Outbox.Batches[0].Topic);
        Assert.Equal("whyce.operational.sandbox.todo.events", host.Outbox.Batches[1].Topic);
        Assert.Contains(host.Outbox.Batches[0].Events, e => e is PolicyEvaluatedEvent);
    }

    [Fact]
    public async Task Deny_Emits_PolicyDeniedEvent_To_Dedicated_Audit_Stream()
    {
        // PolicyMiddleware deny path returns Failure with an AuditEmission;
        // RuntimeControlPlane routes it through ProcessAuditAsync to the
        // dedicated policy decision stream. PolicyDecisionHash is still
        // mandatory — audit emission records governed denials, not bypasses.
        var host = TestHost.ForTodo(denyPolicy: true);
        var aggregateId = host.IdGenerator.Generate("policy:deny:1");
        var context = host.NewTodoContext(aggregateId);

        var result = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Blocked"),
            context);

        Assert.False(result.IsSuccess);

        // Todo aggregate stream is empty — command never reached the engine.
        Assert.Empty(host.EventStore.AllEvents(aggregateId));

        // PolicyDeniedEvent persisted to the dedicated audit stream.
        var auditStreamId = host.IdGenerator.Generate($"policy-audit-stream:{context.CommandId}");
        var auditStream = host.EventStore.AllEvents(auditStreamId);
        var deniedEvent = Assert.IsType<PolicyDeniedEvent>(Assert.Single(auditStream));

        Assert.False(string.IsNullOrEmpty(deniedEvent.DecisionHash));
        Assert.Equal(context.PolicyDecisionHash, deniedEvent.DecisionHash);
        Assert.Equal(context.CorrelationId, deniedEvent.CorrelationId);
        Assert.Equal(context.CausationId, deniedEvent.CausationId);

        Assert.Single(host.ChainAnchor.Blocks);
        var batch = Assert.Single(host.Outbox.Batches);
        Assert.Equal("whyce.constitutional.policy.decision.events", batch.Topic);
        Assert.IsType<PolicyDeniedEvent>(Assert.Single(batch.Events));
    }

    [Fact]
    public async Task Determinism_Same_Command_Produces_Identical_PolicyEvent_EventId()
    {
        // Replay determinism: identical inputs through the deterministic
        // IIdGenerator must yield identical PolicyEvaluatedEvent.EventId.
        var hostA = TestHost.ForTodo();
        var hostB = TestHost.ForTodo();
        var aggregateId = hostA.IdGenerator.Generate("policy:deterministic:1");

        var ctxA = hostA.NewTodoContext(aggregateId);
        var ctxB = hostB.NewTodoContext(aggregateId);

        // Both contexts must share the same CommandId for the seed to match —
        // NewTodoContext derives CommandId deterministically from aggregateId.
        Assert.Equal(ctxA.CommandId, ctxB.CommandId);

        await hostA.ControlPlane.ExecuteAsync(new CreateTodoCommand(aggregateId, "x"), ctxA);
        await hostB.ControlPlane.ExecuteAsync(new CreateTodoCommand(aggregateId, "x"), ctxB);

        var auditStreamId = hostA.IdGenerator.Generate($"policy-audit-stream:{ctxA.CommandId}");
        var a = Assert.IsType<PolicyEvaluatedEvent>(hostA.EventStore.AllEvents(auditStreamId)[0]);
        var b = Assert.IsType<PolicyEvaluatedEvent>(hostB.EventStore.AllEvents(auditStreamId)[0]);

        Assert.Equal(a.EventId, b.EventId);
        Assert.Equal(a.DecisionHash, b.DecisionHash);
    }
}
