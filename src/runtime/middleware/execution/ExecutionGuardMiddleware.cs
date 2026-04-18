using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.Execution;

/// <summary>
/// Final gate before command dispatch to engine.
/// Marks the command context as runtime-originated (RuntimeOriginKey).
/// Ensures all preceding guards and policy evaluation have passed.
/// No command reaches an engine without this guard approving it.
///
/// Phase-4 enforcement integration: when an <see cref="IViolationStateQuery"/>
/// is wired, the guard consults the active-violation projection for the
/// resolved IdentityId and either (a) rejects the command if the subject
/// has any active Block violation, or (b) stamps
/// <see cref="CommandContext.EnforcementConstraint"/> with the active
/// Restrict / Warn / Escalate action so engine handlers can degrade
/// behavior. The query is optional — hosts without enforcement wiring
/// retain the pre-phase-4 behavior verbatim.
///
/// E3 enforcement-to-transaction blocking: when <see cref="IRestrictionStateQuery"/>
/// or <see cref="ILockStateQuery"/> are wired, the guard additionally
/// checks for active restrictions and locks BEFORE any command executes.
/// A lock is a hard stop (reject). A restriction stamps the constraint so
/// handlers can degrade. Evaluation order: Lock → Violation → Restriction → Escalation.
///
/// FAIL-CLOSED HARDENING: Lock checks are FAIL-CLOSED. When lock state
/// cannot be verified (infrastructure failure), command execution is
/// blocked. This is a deliberate asymmetry: violations/restrictions are
/// fail-open (availability), but locks are fail-closed (financial safety).
/// </summary>
public sealed class ExecutionGuardMiddleware : IMiddleware
{
    public const string RuntimeOriginKey = "Runtime.IsFromRuntime";

    private static readonly Meter EnforcementMeter = new("Whycespace.Runtime.Enforcement", "1.0.0");
    private static readonly Counter<long> RestrictionTriggered =
        EnforcementMeter.CreateCounter<long>("whyce.enforcement.restriction.triggered", "events", "Restriction constraints applied");
    private static readonly Counter<long> EnforcementActions =
        EnforcementMeter.CreateCounter<long>("whyce.enforcement.actions.count", "events", "Enforcement actions taken (block/restrict/escalate)");
    private static readonly Counter<long> LockUnavailableBlocks =
        EnforcementMeter.CreateCounter<long>("whyce.enforcement.lock.unavailable_blocks", "events", "Commands blocked due to lock state unavailable");

    private static readonly Counter<long> CacheHits =
        EnforcementMeter.CreateCounter<long>("whyce.enforcement.cache.hits", "events", "Enforcement decisions resolved from cache (projection lag protection)");

    private readonly IViolationStateQuery? _violationStateQuery;
    private readonly IEscalationStateQuery? _escalationStateQuery;
    private readonly IRestrictionStateQuery? _restrictionStateQuery;
    private readonly ILockStateQuery? _lockStateQuery;
    private readonly IEnforcementDecisionCache? _decisionCache;
    private readonly ILogger<ExecutionGuardMiddleware>? _logger;

    public ExecutionGuardMiddleware()
    {
        _violationStateQuery = null;
        _escalationStateQuery = null;
        _restrictionStateQuery = null;
        _lockStateQuery = null;
        _decisionCache = null;
        _logger = null;
    }

    public ExecutionGuardMiddleware(IViolationStateQuery? violationStateQuery)
        : this(violationStateQuery, null, null, null, null, null) { }

    public ExecutionGuardMiddleware(
        IViolationStateQuery? violationStateQuery,
        IEscalationStateQuery? escalationStateQuery)
        : this(violationStateQuery, escalationStateQuery, null, null, null, null) { }

    public ExecutionGuardMiddleware(
        IViolationStateQuery? violationStateQuery,
        IEscalationStateQuery? escalationStateQuery,
        IRestrictionStateQuery? restrictionStateQuery,
        ILockStateQuery? lockStateQuery)
        : this(violationStateQuery, escalationStateQuery, restrictionStateQuery, lockStateQuery, null, null) { }

    public ExecutionGuardMiddleware(
        IViolationStateQuery? violationStateQuery,
        IEscalationStateQuery? escalationStateQuery,
        IRestrictionStateQuery? restrictionStateQuery,
        ILockStateQuery? lockStateQuery,
        ILogger<ExecutionGuardMiddleware>? logger)
        : this(violationStateQuery, escalationStateQuery, restrictionStateQuery, lockStateQuery, null, logger) { }

    public ExecutionGuardMiddleware(
        IViolationStateQuery? violationStateQuery,
        IEscalationStateQuery? escalationStateQuery,
        IRestrictionStateQuery? restrictionStateQuery,
        ILockStateQuery? lockStateQuery,
        IEnforcementDecisionCache? decisionCache,
        ILogger<ExecutionGuardMiddleware>? logger)
    {
        _violationStateQuery = violationStateQuery;
        _escalationStateQuery = escalationStateQuery;
        _restrictionStateQuery = restrictionStateQuery;
        _lockStateQuery = lockStateQuery;
        _decisionCache = decisionCache;
        _logger = logger;
    }

    public async Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        // Final validation: policy decision must have been evaluated
        if (context.PolicyDecisionAllowed is not true)
        {
            return CommandResult.Failure(
                "Execution guard: command cannot proceed without approved policy decision.");
        }

        if (string.IsNullOrWhiteSpace(context.PolicyDecisionHash))
        {
            return CommandResult.Failure(
                "Execution guard: policy decision hash is required for chain anchoring.");
        }

        var commandType = command.GetType().Name;
        var hasSubject = Guid.TryParse(context.IdentityId, out var subjectId) && subjectId != Guid.Empty;

        // PROJECTION LAG PROTECTION: Consult the in-memory decision cache
        // BEFORE the projection queries. The cache is populated directly
        // from the event stream by enforcement handlers, so it reflects
        // enforcement state within milliseconds — closing the window
        // between event emission and projection materialization.
        //
        // E3: Lock check — FAIL-CLOSED hard stop. Evaluated first because a
        // locked subject cannot execute ANY command regardless of
        // violation/restriction state.
        if (hasSubject)
        {
            // Cache-first lock check (projection lag protection).
            var cachedLock = _decisionCache?.TryGetLock(subjectId);
            if (cachedLock is not null)
            {
                CacheHits.Add(1, new("type", "lock"), new("command.type", commandType));

                if (cachedLock.IsLocked)
                {
                    EnforcementActions.Add(1, new("action", "lock_block_cached"), new("command.type", commandType));
                    return CommandResult.Failure(
                        $"Execution guard: subject is locked (scope={cachedLock.Scope}, source=cache). " +
                        "All commands rejected until lock is released.");
                }
            }
        }

        if (_lockStateQuery is not null && hasSubject)
        {
            var lockState = await _lockStateQuery.QueryBySubjectAsync(subjectId, cancellationToken);

            if (lockState.IsUnavailable)
            {
                LockUnavailableBlocks.Add(1, new KeyValuePair<string, object?>("command.type", commandType));
                EnforcementActions.Add(1, new("action", "lock_unavailable_block"), new("command.type", commandType));

                _logger?.LogError(
                    "ENFORCEMENT FAIL-CLOSED: Lock state unavailable for subject {SubjectId}. " +
                    "Blocking {CommandType} (CommandId={CommandId}, CorrelationId={CorrelationId}). " +
                    "Lock verification is required before command execution can proceed.",
                    subjectId, commandType, context.CommandId, context.CorrelationId);

                return CommandResult.Failure(
                    "Execution guard: lock state cannot be verified (infrastructure unavailable). " +
                    "Command blocked — fail-closed. Retry when lock projection is available.");
            }

            if (lockState.IsLocked)
            {
                EnforcementActions.Add(1, new("action", "lock_block"), new("command.type", commandType));

                _logger?.LogWarning(
                    "ENFORCEMENT BLOCK: Subject {SubjectId} is locked (scope={Scope}). " +
                    "Blocking {CommandType} (CommandId={CommandId}).",
                    subjectId, lockState.Scope, commandType, context.CommandId);

                return CommandResult.Failure(
                    $"Execution guard: subject is locked (scope={lockState.Scope}). " +
                    "All commands rejected until lock is released.");
            }
        }

        // Cache-first violation check (projection lag protection).
        if (hasSubject)
        {
            var cachedViolation = _decisionCache?.TryGetViolation(subjectId);
            if (cachedViolation is not null)
            {
                CacheHits.Add(1, new("type", "violation"), new("command.type", commandType));

                if (cachedViolation.IsBlocked)
                {
                    EnforcementActions.Add(1, new("action", "violation_block_cached"), new("command.type", commandType));
                    return CommandResult.Failure(
                        "Execution guard: subject has an active Critical+Block enforcement violation (source=cache). Command rejected.");
                }
                if (!string.IsNullOrEmpty(cachedViolation.Constraint))
                {
                    context.EnforcementConstraint = cachedViolation.Constraint;
                }
            }
        }

        if (_violationStateQuery is not null && hasSubject)
        {
            var posture = await _violationStateQuery.QueryBySubjectAsync(subjectId, cancellationToken);
            if (posture.IsBlocked)
            {
                EnforcementActions.Add(1, new("action", "violation_block"), new("command.type", commandType));

                return CommandResult.Failure(
                    "Execution guard: subject has an active Critical+Block enforcement violation. Command rejected.");
            }
            if (!string.IsNullOrEmpty(posture.Constraint) && string.IsNullOrEmpty(context.EnforcementConstraint))
            {
                context.EnforcementConstraint = posture.Constraint;
            }
        }

        // Phase 2 — Restriction check. LOCKED SEMANTICS: HARD REJECT.
        // A restricted subject cannot execute any gated command. The
        // middleware rejects before engine dispatch so no events are
        // emitted and no workflow step progresses. Defense-in-depth: the
        // engine handlers ALSO call EnforcementGuard.RequireNotRestricted
        // on the stamped constraint so any path that bypasses this
        // middleware (e.g. workflow recovery) still enforces.
        //
        // Cache-first (projection-lag protection): the restriction cache
        // is populated synchronously by ApplyRestrictionHandler so the
        // very next command for the same subject sees the restriction
        // without waiting for projection materialization.
        if (hasSubject)
        {
            var cachedRestriction = _decisionCache?.TryGetRestriction(subjectId);
            if (cachedRestriction is not null && cachedRestriction.IsRestricted)
            {
                CacheHits.Add(1,
                    new KeyValuePair<string, object?>("type", "restriction"),
                    new KeyValuePair<string, object?>("command.type", commandType));
                RestrictionTriggered.Add(1,
                    new KeyValuePair<string, object?>("command.type", commandType),
                    new KeyValuePair<string, object?>("scope", cachedRestriction.Scope ?? "unknown"));
                EnforcementActions.Add(1,
                    new KeyValuePair<string, object?>("action", "restriction_block_cached"),
                    new KeyValuePair<string, object?>("command.type", commandType));

                context.EnforcementConstraint = $"Restricted:{cachedRestriction.Scope}";
                return CommandResult.Failure(
                    $"Execution guard: subject is restricted (scope={cachedRestriction.Scope}, source=cache). " +
                    "Command rejected.");
            }
        }

        if (_restrictionStateQuery is not null && hasSubject)
        {
            var restriction = await _restrictionStateQuery.QueryBySubjectAsync(subjectId, cancellationToken);
            if (restriction.IsRestricted)
            {
                RestrictionTriggered.Add(1,
                    new KeyValuePair<string, object?>("command.type", commandType),
                    new KeyValuePair<string, object?>("scope", restriction.Scope ?? "unknown"));
                EnforcementActions.Add(1,
                    new KeyValuePair<string, object?>("action", "restriction_block"),
                    new KeyValuePair<string, object?>("command.type", commandType));

                context.EnforcementConstraint = $"Restricted:{restriction.Scope}";
                return CommandResult.Failure(
                    $"Execution guard: subject is restricted (scope={restriction.Scope}). " +
                    "Command rejected.");
            }
        }

        if (_escalationStateQuery is not null && hasSubject)
        {
            var escalation = await _escalationStateQuery.QueryBySubjectAsync(subjectId, cancellationToken);
            if (escalation.IsCritical)
            {
                EnforcementActions.Add(1, new("action", "escalation_block"), new("command.type", commandType));

                return CommandResult.Failure(
                    "Execution guard: subject escalation level is Critical. Command rejected.");
            }
            if (escalation.IsHigh || escalation.IsMedium)
            {
                EnforcementActions.Add(1, new("action", $"escalation_{escalation.Level.ToLowerInvariant()}"), new("command.type", commandType));

                // Stamp only when a stronger constraint from the violation layer
                // has not already been set; High overrides Medium.
                if (string.IsNullOrEmpty(context.EnforcementConstraint)
                    || string.Equals(context.EnforcementConstraint, "Medium", StringComparison.Ordinal))
                {
                    context.EnforcementConstraint = escalation.Level;
                }
            }
        }

        // Mark context as runtime-originated — engines can verify this
        context.RuntimeOrigin = true;

        return await next(cancellationToken);
    }
}
