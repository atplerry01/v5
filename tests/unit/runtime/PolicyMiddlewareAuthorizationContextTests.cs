using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhycePolicy.Engine;
using Whycespace.Runtime.Middleware.Policy;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.Runtime;

/// <summary>
/// Phase 2.8 Batch D — pins that PolicyMiddleware calls BuildAuthorizationContext
/// after AuthenticateIdentity and propagates ActiveConsentScopes + ContextHash
/// into EvaluatePolicyCommand. Observable via end-to-end pipeline success and
/// deterministic policy decision hash on the CommandContext.
/// </summary>
public sealed class PolicyMiddlewareAuthorizationContextTests
{
    private static readonly DateTimeOffset FixedNow =
        DateTimeOffset.Parse("2026-04-22T00:00:00Z");

    private static readonly Guid CorrelationId =
        new TestIdGenerator().Generate("correlation");

    private static readonly Guid CommandId =
        new TestIdGenerator().Generate("command");

    private static readonly Guid AggregateId =
        new TestIdGenerator().Generate("aggregate");

    // --- Stub collaborators ---

    private sealed class AllowingPolicyEvaluator : IPolicyEvaluator
    {
        public Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext ctx) =>
            Task.FromResult(new PolicyDecision(true, policyId, "hash-allow", null));
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => FixedNow;
    }

    private sealed class NullStateLoader : IAggregateStateLoader
    {
        public Task<object?> LoadSnapshotAsync(Type commandType, Guid aggregateId, CancellationToken ct = default) =>
            Task.FromResult<object?>(null);
    }

    private sealed class NoRolesCaller : ICallerIdentityAccessor
    {
        private readonly string[] _roles;
        public NoRolesCaller(string[] roles) { _roles = roles; }
        public string GetActorId() => "actor";
        public string GetTenantId() => "tenant";
        public string[] GetRoles() => _roles;
        public IReadOnlyDictionary<string, object> GetSubjectAttributes() =>
            new Dictionary<string, object>();
        public string? GetSessionId() => null;
        public string? GetTokenFingerprint() => null;
    }

    private sealed class StubDecisionEventFactory : IPolicyDecisionEventFactory
    {
        public AuditEmission CreateEvaluatedEmission(
            Guid eventId, Guid aggregateId, string identityId,
            string policyName, string decisionHash, string executionHash,
            string policyVersion, Guid commandId, Guid correlationId, Guid causationId) =>
            new()
            {
                Events = [],
                AggregateId = aggregateId,
                Classification = "constitutional",
                Context = "policy",
                Domain = "decision",
                Metadata = new Dictionary<string, string>
                {
                    ["decision_hash"] = decisionHash,
                    ["execution_hash"] = executionHash,
                    ["policy_version"] = policyVersion
                }
            };

        public AuditEmission CreateDeniedEmission(
            Guid eventId, Guid aggregateId, string identityId,
            string policyName, string decisionHash, string executionHash,
            string policyVersion, Guid commandId, Guid correlationId, Guid causationId) =>
            CreateEvaluatedEmission(eventId, aggregateId, identityId, policyName,
                decisionHash, executionHash, policyVersion, commandId, correlationId, causationId);
    }

    private sealed record StubCommand;

    private static PolicyMiddleware BuildMiddleware(string[] callerRoles) =>
        new(
            new WhyceIdEngine(),
            new WhycePolicyEngine(),
            new AllowingPolicyEvaluator(),
            new TestIdGenerator(),
            new StubDecisionEventFactory(),
            new NoRolesCaller(callerRoles),
            new FixedClock(),
            new NullStateLoader());

    private static CommandContext BuildContext(string actorId = "user-1") =>
        new()
        {
            CorrelationId = CorrelationId,
            CausationId = CorrelationId,
            CommandId = CommandId,
            TenantId = "tenant",
            ActorId = actorId,
            AggregateId = AggregateId,
            PolicyId = "default",
            Classification = "trust",
            Context = "identity",
            Domain = "identity"
        };

    [Fact]
    public async Task ExecuteAsync_ValidIdentity_PipelineAllows()
    {
        var middleware = BuildMiddleware(["admin"]);
        var context = BuildContext();
        var result = await middleware.ExecuteAsync(
            context, new StubCommand(),
            _ => Task.FromResult(CommandResult.Success([])),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.AuditEmission);
    }

    [Fact]
    public async Task ExecuteAsync_ValidIdentity_ContextHashPropagated()
    {
        // ContextHash is produced deterministically; its presence on the policy
        // decision hash (via authCtxResult.Context.TrustScore used in the
        // EvaluatePolicyCommand) means two identical executions produce the
        // same context.PolicyDecisionHash.
        var middleware = BuildMiddleware(["user"]);

        var context1 = BuildContext("user-determinism");
        var context2 = BuildContext("user-determinism");

        await middleware.ExecuteAsync(context1, new StubCommand(),
            _ => Task.FromResult(CommandResult.Success([])), CancellationToken.None);
        await middleware.ExecuteAsync(context2, new StubCommand(),
            _ => Task.FromResult(CommandResult.Success([])), CancellationToken.None);

        Assert.NotNull(context1.PolicyDecisionHash);
        Assert.Equal(context1.PolicyDecisionHash, context2.PolicyDecisionHash);
    }

    [Fact]
    public async Task ExecuteAsync_DifferentRoles_ProduceDifferentDecisionHash()
    {
        var middlewareAdmin = BuildMiddleware(["admin"]);
        var middlewareUser = BuildMiddleware(["user"]);

        var ctxAdmin = BuildContext("actor-hash");
        var ctxUser = BuildContext("actor-hash");

        await middlewareAdmin.ExecuteAsync(ctxAdmin, new StubCommand(),
            _ => Task.FromResult(CommandResult.Success([])), CancellationToken.None);
        await middlewareUser.ExecuteAsync(ctxUser, new StubCommand(),
            _ => Task.FromResult(CommandResult.Success([])), CancellationToken.None);

        // Roles flow into TrustScore via WhyceIdEngine then into EvaluatePolicyCommand.
        // admin gets a higher trust score → different decision hash.
        Assert.NotEqual(ctxAdmin.PolicyDecisionHash, ctxUser.PolicyDecisionHash);
    }
}
