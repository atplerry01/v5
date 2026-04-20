using Whycespace.Engines.T0U.WhyceId.Command;
using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhycePolicy.Command;
using Whycespace.Engines.T0U.WhycePolicy.Engine;
using Whycespace.Runtime.Deterministic;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.Middleware.Policy;

/// <summary>
/// WHYCEPOLICY enforcement middleware. Every command MUST pass through policy evaluation.
/// Policy deny = HARD STOP. No engine execution without explicit policy approval.
///
/// Mandatory Flow (T0U Constitutional):
/// 1. WhyceId → resolve identity (AuthenticateIdentity)
/// 2. OPA → evaluate external policy (IPolicyEvaluator)
/// 3. WhycePolicy → evaluate constitutional compliance (Evaluate)
/// 4. Inject identity + policy decision into context
/// 5. Deny → halt, Allow → proceed
///
/// Non-bypassable: No request without WhyceId. No execution without WhycePolicy.
/// </summary>
public sealed class PolicyMiddleware : IMiddleware
{
    private readonly WhyceIdEngine _whyceIdEngine;
    private readonly WhycePolicyEngine _whycePolicyEngine;
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IIdGenerator _idGenerator;
    private readonly IPolicyDecisionEventFactory _decisionEventFactory;
    private readonly ICallerIdentityAccessor _callerIdentity;
    private readonly IClock _clock;
    private readonly IAggregateStateLoader _aggregateStateLoader;

    // R2.A.OPA / R-POL-OPA-RETRY-01 — optional retry executor. When registered
    // (host composition via CoreComposition), OPA unavailable exceptions
    // (timeout / transport / http_status / breaker_open) are caught and
    // classified as RuntimeFailureCategory.PolicyEvaluationDeferred inside a
    // bounded retry loop. Exhausted retries re-throw so the API edge's
    // existing 503 + Retry-After handler still fires. When null (legacy
    // tests / background workers without the executor), the pre-R2.A.OPA
    // behaviour is preserved verbatim — the exception bubbles on the first
    // attempt.
    private readonly IRetryExecutor? _retryExecutor;
    private static readonly RetryPolicy OpaRetryPolicy = new()
    {
        MaxAttempts = 3,
        InitialDelayMs = 200
    };

    public PolicyMiddleware(
        WhyceIdEngine whyceIdEngine,
        WhycePolicyEngine whycePolicyEngine,
        IPolicyEvaluator policyEvaluator,
        IIdGenerator idGenerator,
        IPolicyDecisionEventFactory decisionEventFactory,
        ICallerIdentityAccessor callerIdentity,
        IClock clock,
        IAggregateStateLoader aggregateStateLoader)
        : this(whyceIdEngine, whycePolicyEngine, policyEvaluator, idGenerator,
               decisionEventFactory, callerIdentity, clock, aggregateStateLoader,
               retryExecutor: null)
    {
    }

    public PolicyMiddleware(
        WhyceIdEngine whyceIdEngine,
        WhycePolicyEngine whycePolicyEngine,
        IPolicyEvaluator policyEvaluator,
        IIdGenerator idGenerator,
        IPolicyDecisionEventFactory decisionEventFactory,
        ICallerIdentityAccessor callerIdentity,
        IClock clock,
        IAggregateStateLoader aggregateStateLoader,
        IRetryExecutor? retryExecutor)
    {
        _whyceIdEngine = whyceIdEngine;
        _whycePolicyEngine = whycePolicyEngine;
        _policyEvaluator = policyEvaluator;
        _idGenerator = idGenerator;
        _decisionEventFactory = decisionEventFactory;
        _callerIdentity = callerIdentity;
        _clock = clock;
        _aggregateStateLoader = aggregateStateLoader;
        _retryExecutor = retryExecutor;
    }

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Resolve identity via WhyceIdEngine (T0U).
        // Pull JWT role claims from the authenticated principal at the HTTP
        // boundary and forward them into the engine. Empty / absent claims
        // leave Roles null so the engine retains its safe-default behaviour.
        // GetRoles() may throw when invoked outside an HTTP context (e.g.
        // worker-driven internal dispatch); treat that as "no claims" rather
        // than failing the whole pipeline so non-HTTP callers continue to
        // resolve to the default role.
        string[]? callerRoles = null;
        IReadOnlyDictionary<string, object>? subjectAttributes = null;
        try
        {
            callerRoles = _callerIdentity.GetRoles();
            subjectAttributes = _callerIdentity.GetSubjectAttributes();
        }
        catch (InvalidOperationException) { /* no HTTP context — leave null */ }

        var authenticateCommand = new AuthenticateIdentityCommand(
            Token: null,
            UserId: context.ActorId,
            DeviceId: null,
            Roles: callerRoles);

        var authResult = await _whyceIdEngine.AuthenticateIdentity(authenticateCommand);

        if (!authResult.IsAuthenticated)
        {
            return CommandResult.Failure(
                "Identity resolution failed. Policy enforcement requires valid identity. No bypass allowed.",
                RuntimeFailureCategory.AuthorizationDenied);
        }

        // Step 2: Inject identity into ExecutionContext
        context.IdentityId = authResult.Identity.IdentityId;
        context.Roles = authResult.Identity.Roles;
        context.TrustScore = authResult.Identity.TrustScore;

        // Step 3: Evaluate external policy via OPA (IPolicyEvaluator).
        //
        // Phase 8 B6 — input enrichment. Every evaluation carries:
        //   * input.command            — the typed command record, serialised with
        //                                the canonical snake-case policy so rego
        //                                can dereference input.command.counterparty_id
        //                                etc. uniformly;
        //   * input.resource.state     — the aggregate snapshot hydrated via
        //                                IAggregateStateLoader (null when no loader
        //                                is registered for this command type, which
        //                                is the pre-B6 behaviour);
        //   * input.now / input.now_ns — a single IClock.UtcNow read stamped here,
        //                                forwarded through PolicyContext so the OPA
        //                                adapter does not re-read the clock.
        //
        // Passing `now` explicitly (rather than letting the adapter source it)
        // keeps replay-determinism at the middleware boundary: reruns of the
        // same command with the same upstream state produce the same OPA input.
        var now = _clock.UtcNow;

        object? resourceState = null;
        var aggregateId = PolicyInputBuilder.TryResolveAggregateId(command);
        if (aggregateId is { } aid)
        {
            // The loader is responsible for hydrating from the same
            // authoritative source the engine would use (IEventStore replay,
            // not a projection read) so policy sees what the system will
            // act on — not a projection-lag-skewed view.
            resourceState = await _aggregateStateLoader.LoadSnapshotAsync(
                command.GetType(), aid, cancellationToken);
        }

        var baseContext = new PolicyContext(
            context.CorrelationId,
            context.TenantId,
            context.ActorId,
            command.GetType().Name,
            authResult.Identity.Roles,
            context.Classification,
            context.Context,
            context.Domain,
            subjectAttributes);

        // R1 §6 — jurisdiction/environment overlays. Contract-level support
        // is in place; a dedicated overlay source (host environment name +
        // tenant → jurisdiction resolver) has not yet been wired, so both
        // are passed as null for now and the 6-arg overload emits them as
        // omitted input fields. When overlay sources land, swap the nulls
        // for the resolved values — rego policies consume `input.environment`
        // and `input.jurisdiction` via the canonical snake-case envelope.
        var opaContext = PolicyInputBuilder.Enrich(
            baseContext, command, resourceState, now,
            environment: null,
            jurisdiction: null);

        var opaDecision = _retryExecutor is null
            ? await _policyEvaluator.EvaluateAsync(context.PolicyId, command, opaContext)
            : await EvaluateOpaWithRetryAsync(context, command, opaContext, cancellationToken);
        if (!opaDecision.IsAllowed)
        {
            // OPA-deny is also a governed decision and MUST emit audit. The
            // OPA decision hash is treated as canonical for this branch since
            // the constitutional WhycePolicyEngine never runs.
            context.PolicyDecisionAllowed = false;
            context.PolicyDecisionHash = opaDecision.DecisionHash;
            context.PolicyVersion = opaDecision.PolicyId;

            var opaAuditAggregateId = _idGenerator.Generate($"policy-audit-stream:{context.CommandId}");
            var opaDenyEmission = _decisionEventFactory.CreateDeniedEmission(
                eventId: _idGenerator.Generate($"{context.CommandId}:PolicyDeniedEvent"),
                aggregateId: opaAuditAggregateId,
                identityId: authResult.Identity.IdentityId,
                policyName: context.PolicyId,
                decisionHash: opaDecision.DecisionHash,
                executionHash: ExecutionHash.Compute(context, Array.Empty<object>()),
                policyVersion: opaDecision.PolicyId,
                commandId: context.CommandId,
                correlationId: context.CorrelationId,
                causationId: context.CausationId);

            // Phase 8 B6 — structured deny reason propagation. The Error
            // string keeps its free-form caller-facing shape; PolicyDenyReason
            // carries the same token(s) as a dedicated machine-readable field
            // for audit correlation and downstream rego rule attribution.
            return CommandResult.Failure(
                $"OPA policy denied: {opaDecision.DenialReason ?? "external policy evaluation failed"}. No bypass allowed.",
                RuntimeFailureCategory.PolicyDenied)
                with
                {
                    AuditEmission = opaDenyEmission,
                    PolicyDenyReason = opaDecision.DenialReason
                };
        }

        // Step 4: Evaluate constitutional policy via WhycePolicyEngine (T0U)
        var evaluateCommand = new EvaluatePolicyCommand(
            PolicyName: context.PolicyId,
            IdentityId: authResult.Identity.IdentityId,
            Roles: authResult.Identity.Roles,
            TrustScore: authResult.Identity.TrustScore,
            CommandType: command.GetType().Name,
            TenantId: context.TenantId,
            ResourceId: null);

        var policyResult = await _whycePolicyEngine.Evaluate(evaluateCommand);

        // Step 4: Inject PolicyDecision into ExecutionContext (write-once — locked after this)
        context.PolicyDecisionAllowed = policyResult.IsCompliant;
        context.PolicyDecisionHash = policyResult.DecisionHash;
        context.PolicyVersion = policyResult.PolicyVersion;

        // Step 5: Build the dedicated audit stream coordinate. Deterministic
        // seed → IIdGenerator → Guid. Same seed yields the same aggregate id
        // on replay so the audit stream is reproducible.
        var auditAggregateId = _idGenerator.Generate($"policy-audit-stream:{context.CommandId}");
        var policyVersion = policyResult.PolicyVersion ?? "none";

        // Step 6: Policy deny = HARD STOP. The denial is recorded via an
        // AuditEmission so it flows to the dedicated policy decision stream
        // (constitutional/policy/decision) — independent of the command's
        // aggregate. Satisfies POL-AUDIT-01 / POLICY-NO-SILENT-DECISION-01.
        if (!policyResult.IsCompliant)
        {
            var denyEmission = _decisionEventFactory.CreateDeniedEmission(
                eventId: _idGenerator.Generate($"{context.CommandId}:PolicyDeniedEvent"),
                aggregateId: auditAggregateId,
                identityId: authResult.Identity.IdentityId,
                policyName: context.PolicyId,
                decisionHash: policyResult.DecisionHash,
                executionHash: ExecutionHash.Compute(context, Array.Empty<object>()),
                policyVersion: policyVersion,
                commandId: context.CommandId,
                correlationId: context.CorrelationId,
                causationId: context.CausationId);

            // Phase 8 B6 — structured deny reason propagation mirrors the
            // OPA-deny branch: Error stays free-form, PolicyDenyReason
            // carries the same token for audit correlation.
            return CommandResult.Failure(
                $"WHYCEPOLICY denied: {policyResult.DenialReason ?? "execution not permitted"}. No bypass allowed.",
                RuntimeFailureCategory.PolicyDenied)
                with
                {
                    AuditEmission = denyEmission,
                    PolicyDenyReason = policyResult.DenialReason
                };
        }

        // Step 7: ALLOW path. Build the AuditEmission BEFORE dispatching the
        // remainder of the pipeline so the audit record is unconditional —
        // attached even if a downstream middleware or the engine fails. The
        // fabric routes it to the dedicated policy decision stream (NOT the
        // command's aggregate stream). Determinism: every field is sourced
        // from upstream context; EventId is derived deterministically from
        // CommandId so replay reproduces the identical record.
        var allowEmission = _decisionEventFactory.CreateEvaluatedEmission(
            eventId: _idGenerator.Generate($"{context.CommandId}:PolicyEvaluatedEvent"),
            aggregateId: auditAggregateId,
            identityId: authResult.Identity.IdentityId,
            policyName: context.PolicyId,
            decisionHash: policyResult.DecisionHash,
            executionHash: ExecutionHash.Compute(context, Array.Empty<object>()),
            policyVersion: policyVersion,
            commandId: context.CommandId,
            correlationId: context.CorrelationId,
            causationId: context.CausationId);

        var innerResult = await next(cancellationToken);

        // Audit emission overrides any AuditEmission set downstream — policy
        // decision is the canonical audit for this execution.
        return innerResult with { AuditEmission = allowEmission };
    }

    /// <summary>
    /// R2.A.OPA / R-POL-OPA-RETRY-01 — bounded retry around the OPA evaluator
    /// call. On <see cref="PolicyEvaluationUnavailableException"/> the retry
    /// step is classified as <see cref="RuntimeFailureCategory.PolicyEvaluationDeferred"/>
    /// which <c>RetryEligibility</c> treats as retryable. On exhaustion, the
    /// last captured exception is re-thrown with <c>reason="retry_exhausted:&lt;original&gt;"</c>
    /// and <c>RetryAfterSeconds</c> preserved, so the API edge's existing
    /// 503 + Retry-After handler surfaces the failure as before. Normal
    /// allow / deny decisions bypass the retry loop on the first attempt.
    /// </summary>
    private async Task<PolicyDecision> EvaluateOpaWithRetryAsync(
        CommandContext context,
        object command,
        PolicyContext opaContext,
        CancellationToken cancellationToken)
    {
        // Captured so we can re-throw with the ORIGINAL RetryAfterSeconds on
        // exhaustion — RetryStepResult carries only (category, error_string),
        // not the typed exception.
        PolicyEvaluationUnavailableException? lastUnavailable = null;

        var retryCtx = new RetryOperationContext
        {
            OperationId = $"{context.CorrelationId:N}:opa:{context.PolicyId}",
            Policy = OpaRetryPolicy,
            OperationName = "opa-evaluate"
        };

        var retryResult = await _retryExecutor!.ExecuteAsync<PolicyDecision>(
            retryCtx,
            async (attempt, ct) =>
            {
                try
                {
                    var decision = await _policyEvaluator.EvaluateAsync(
                        context.PolicyId, command, opaContext);
                    return RetryStepResult<PolicyDecision>.Success(decision);
                }
                catch (PolicyEvaluationUnavailableException ex)
                {
                    lastUnavailable = ex;
                    return RetryStepResult<PolicyDecision>.Failure(
                        RuntimeFailureCategory.PolicyEvaluationDeferred,
                        $"{ex.Reason}: {ex.Message}");
                }
            },
            cancellationToken);

        if (retryResult.Outcome == RetryOutcome.Success)
        {
            return retryResult.Value!;
        }

        // R-POL-OPA-RETRY-01 exhaustion path: OPA unavailable for all
        // attempts → re-throw so API edge 503+Retry-After handler fires.
        // The POL-FAIL-CLASS-01 FAIL_CLOSED invariant is satisfied via the
        // rejection path — the engine never sees the command.
        if (lastUnavailable is not null)
        {
            throw new PolicyEvaluationUnavailableException(
                reason: $"retry_exhausted:{lastUnavailable.Reason}",
                retryAfterSeconds: lastUnavailable.RetryAfterSeconds,
                message: $"OPA policy evaluation for '{context.PolicyId}' failed after {retryResult.AttemptsMade} attempts. Last reason: {lastUnavailable.Reason}. {lastUnavailable.Message}",
                innerException: lastUnavailable);
        }

        // Unreachable in practice — a non-success outcome without a captured
        // PolicyEvaluationUnavailableException implies the inner op threw a
        // non-retryable exception that the executor wrapped as
        // ExecutionFailure. Preserve the underlying error.
        throw new InvalidOperationException(
            $"Policy evaluation retry exhausted without captured unavailable exception. " +
            $"Outcome={retryResult.Outcome}, AttemptsMade={retryResult.AttemptsMade}, Error={retryResult.FinalError}");
    }
}
