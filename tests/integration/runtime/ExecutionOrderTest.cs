using Whyce.Shared.Contracts.Application.Todo;
using Whycespace.Tests.Integration.Setup;

namespace Whycespace.Tests.Integration.Runtime;

/// <summary>
/// Witnesses the locked WBSM v3 execution order at runtime by recording each
/// stage as it executes. The 8 middlewares are wrapped in RecordingMiddleware
/// inside TestHost; the fabric stages (Persist, Chain, Outbox) record via the
/// in-memory adapters. Under WBSM v3.5 policy eventification, the fabric runs
/// TWICE per command on the allow path: first for the policy AuditEmission
/// (constitutional-system/policy/decision stream), then for the command's
/// domain events. Audit always precedes domain.
///
/// This test makes the static guarantees in
/// claude/audits/runtime-order.audit.md observable at runtime.
/// </summary>
public sealed class ExecutionOrderTest
{
    [Fact]
    public async Task Locked_8_Middleware_Plus_3_Fabric_Order_Is_Preserved()
    {
        var host = TestHost.ForTodo();
        var aggregateId = host.IdGenerator.Generate("todo:order:1");

        var result = await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Test"),
            host.NewTodoContext(aggregateId));

        Assert.True(result.IsSuccess, result.Error);

        var observed = host.Recorder.Snapshot();

        // Expected canonical order — 8 middlewares + 2 fabric passes.
        // Middleware names match RuntimeControlPlaneBuilder.Build() positions 1..8.
        // Fabric pass 1: AuditEmission (policy decision stream).
        // Fabric pass 2: Domain events (command's aggregate stream).
        var expected = new[]
        {
            "Tracing",
            "Metrics",
            "ContextGuard",
            "Validation",
            "Policy",
            "AuthorizationGuard",
            "Idempotency",
            "ExecutionGuard",
            "Persist",
            "Chain",
            "Outbox",
            "Persist",
            "Chain",
            "Outbox"
        };

        Assert.Equal(expected, observed);
    }

    [Fact]
    public async Task Policy_Sits_Strictly_Between_Pre_And_Post_Policy_Guards()
    {
        var host = TestHost.ForTodo();
        var aggregateId = host.IdGenerator.Generate("todo:order:2");

        await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Test"),
            host.NewTodoContext(aggregateId));

        var observed = host.Recorder.Snapshot();
        var contextGuardIdx = Array.IndexOf(observed.ToArray(), "ContextGuard");
        var validationIdx = Array.IndexOf(observed.ToArray(), "Validation");
        var policyIdx = Array.IndexOf(observed.ToArray(), "Policy");
        var authzIdx = Array.IndexOf(observed.ToArray(), "AuthorizationGuard");
        var idempIdx = Array.IndexOf(observed.ToArray(), "Idempotency");

        Assert.True(contextGuardIdx < policyIdx, "ContextGuard must precede Policy");
        Assert.True(validationIdx < policyIdx, "Validation must precede Policy");
        Assert.True(policyIdx < authzIdx, "Policy must precede AuthorizationGuard");
        Assert.True(policyIdx < idempIdx, "Policy must precede Idempotency");
    }

    [Fact]
    public async Task Persist_Strictly_Precedes_Chain_Strictly_Precedes_Outbox()
    {
        var host = TestHost.ForTodo();
        var aggregateId = host.IdGenerator.Generate("todo:order:3");

        await host.ControlPlane.ExecuteAsync(
            new CreateTodoCommand(aggregateId, "Test"),
            host.NewTodoContext(aggregateId));

        var observed = host.Recorder.Snapshot();
        var persistIdx = Array.IndexOf(observed.ToArray(), "Persist");
        var chainIdx = Array.IndexOf(observed.ToArray(), "Chain");
        var outboxIdx = Array.IndexOf(observed.ToArray(), "Outbox");

        Assert.True(persistIdx >= 0, "Persist stage was not observed");
        Assert.True(chainIdx >= 0, "Chain stage was not observed");
        Assert.True(outboxIdx >= 0, "Outbox stage was not observed");
        Assert.True(persistIdx < chainIdx, "Persist must precede Chain");
        Assert.True(chainIdx < outboxIdx, "Chain must precede Outbox");
    }
}
