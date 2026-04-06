using Whycespace.Platform.Api.Governance.Policy.Contracts;
using Whycespace.Runtime.Bootstrap;
using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Governance.Policy;

public static class PolicyEndpoints
{
    public static WebApplication MapPolicyEndpoints(this WebApplication app)
    {
        // ── Commands ──

        app.MapPost("/api/policy/proposal", async (
            SubmitPolicyProposalRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.proposal.submit:trace:{request.PolicyId}:{request.ProposedBy}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("proposal.submit");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.proposal.submit:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    PolicyId = Guid.Parse(request.PolicyId),
                    ProposedBy = Guid.Parse(request.ProposedBy),
                    request.DslContent
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"policy.proposal.submit:correlation:{traceId}").ToString(),
                Timestamp = request.Timestamp ?? clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "PROPOSAL_SUBMITTED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy");

        app.MapPost("/api/policy/approve", async (
            ApprovePolicyRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.approve:trace:{request.ProposalId}:{request.ApproverId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("approve");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.approve:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    ProposalId = Guid.Parse(request.ProposalId),
                    ApproverId = Guid.Parse(request.ApproverId),
                    request.Decision
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"policy.approve:correlation:{traceId}").ToString(),
                Timestamp = request.Timestamp ?? clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "APPROVED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy");

        app.MapPost("/api/policy/activate", async (
            ActivatePolicyRequest request,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.activate:trace:{request.ProposalId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("activate");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.activate:command:{traceId}"),
                CommandType = commandType,
                Payload = new
                {
                    ProposalId = Guid.Parse(request.ProposalId)
                },
                CorrelationId = request.CommandId ?? idGen.DeterministicGuid($"policy.activate:correlation:{traceId}").ToString(),
                Timestamp = request.Timestamp ?? clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { status = "ACTIVATED", data = result.Data, traceId });

            return Results.BadRequest(new { status = "FAILED", error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy");

        // ── Queries ──

        app.MapGet("/api/policy/{policyId}", async (
            string policyId,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.get:trace:{policyId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("get");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.get:command:{traceId}"),
                CommandType = commandType,
                Payload = new { PolicyId = policyId },
                CorrelationId = idGen.DeterministicGuid($"policy.get:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { data = result.Data, traceId });

            return Results.NotFound(new { error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy");

        app.MapGet("/api/policy/{policyId}/versions", async (
            string policyId,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.versions:trace:{policyId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("versions");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.versions:command:{traceId}"),
                CommandType = commandType,
                Payload = new { PolicyId = policyId },
                CorrelationId = idGen.DeterministicGuid($"policy.versions:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            return Results.Ok(new { data = result.Data, traceId });
        })
            .WithTags("Policy");

        app.MapGet("/api/policy/{policyId}/active", async (
            string policyId,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.active:trace:{policyId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("active");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.active:command:{traceId}"),
                CommandType = commandType,
                Payload = new { PolicyId = policyId },
                CorrelationId = idGen.DeterministicGuid($"policy.active:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            if (result.Success)
                return Results.Ok(new { data = result.Data, traceId });

            return Results.NotFound(new { error = result.ErrorMessage, traceId });
        })
            .WithTags("Policy");

        app.MapGet("/api/policy/{policyId}/history", async (
            string policyId,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.history:trace:{policyId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("history");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.history:command:{traceId}"),
                CommandType = commandType,
                Payload = new { PolicyId = policyId },
                CorrelationId = idGen.DeterministicGuid($"policy.history:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            return Results.Ok(new { data = result.Data, traceId });
        })
            .WithTags("Policy");

        // ── Governance Visibility ──

        app.MapGet("/api/policy/governance/pending", async (
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid("policy.governance.pending:trace").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("governance.pending");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.governance.pending:command:{traceId}"),
                CommandType = commandType,
                Payload = new { Status = "Pending" },
                CorrelationId = idGen.DeterministicGuid($"policy.governance.pending:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            return Results.Ok(new { data = result.Data, traceId });
        })
            .WithTags("Policy Governance");

        app.MapGet("/api/policy/governance/approvals/{proposalId}", async (
            string proposalId,
            IProcessHandlerRegistry registry,
            IIdGenerator idGen,
            IClock clock) =>
        {
            var traceId = idGen.DeterministicGuid($"policy.governance.approvals:trace:{proposalId}").ToString("N");
            var commandType = RuntimeBootstrap.PolicyRoute.ResolveCommandType("governance.approvals");
            var handler = registry.Resolve(commandType);

            var result = await handler.HandleAsync(new ProcessCommand
            {
                CommandId = idGen.DeterministicGuid($"policy.governance.approvals:command:{traceId}"),
                CommandType = commandType,
                Payload = new { ProposalId = proposalId },
                CorrelationId = idGen.DeterministicGuid($"policy.governance.approvals:correlation:{traceId}").ToString(),
                Timestamp = clock.UtcNow
            });

            return Results.Ok(new { data = result.Data, traceId });
        })
            .WithTags("Policy Governance");

        return app;
    }
}
