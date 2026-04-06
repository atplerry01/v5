using Whyce.Runtime.Middleware;
using Whyce.Runtime.Middleware.Execution;
using Whyce.Runtime.Middleware.Observability;
using Whyce.Runtime.Middleware.PostPolicy;
using Whyce.Runtime.Middleware.PrePolicy;
using Whyce.Shared.Contracts.Runtime;
using PolicyMw = Whyce.Runtime.Middleware.Policy.PolicyMiddleware;

namespace Whyce.Runtime.ControlPlane;

/// <summary>
/// Builds the RuntimeControlPlane with LOCKED middleware order.
/// All mandatory middleware must be supplied before Build().
/// Middleware order is canonical and cannot be changed at runtime.
///
/// LOCKED ORDER:
///   1. TracingMiddleware (observability)
///   2. MetricsMiddleware (observability)
///   3. ContextGuardMiddleware (pre-policy)
///   4. ValidationMiddleware (pre-policy)
///   5. PolicyMiddleware (policy — WHYCEPOLICY™)
///   6. AuthorizationGuardMiddleware (post-policy)
///   7. IdempotencyMiddleware (post-policy)
///   8. ExecutionGuardMiddleware (execution — final gate)
///   → CommandDispatcher (terminal)
/// </summary>
public sealed class RuntimeControlPlaneBuilder
{
    private ContextGuardMiddleware? _contextGuard;
    private ValidationMiddleware? _validation;
    private PolicyMw? _policy;
    private AuthorizationGuardMiddleware? _authorizationGuard;
    private IdempotencyMiddleware? _idempotency;
    private TracingMiddleware? _tracing;
    private MetricsMiddleware? _metrics;
    private ExecutionGuardMiddleware? _executionGuard;

    public RuntimeControlPlaneBuilder UseContextGuard(ContextGuardMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _contextGuard = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseValidation(ValidationMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _validation = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UsePolicy(PolicyMw middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _policy = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseAuthorizationGuard(AuthorizationGuardMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _authorizationGuard = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseIdempotency(IdempotencyMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _idempotency = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseTracing(TracingMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _tracing = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseMetrics(MetricsMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _metrics = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseExecutionGuard(ExecutionGuardMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _executionGuard = middleware;
        return this;
    }

    /// <summary>
    /// Builds the middleware pipeline in LOCKED order and returns the ordered list.
    /// All mandatory middleware must have been registered.
    /// </summary>
    public IReadOnlyList<IMiddleware> Build()
    {
        ValidateMandatoryDependencies();

        // ── Middleware order is LOCKED ──
        // This is the canonical execution order per WBSM v3.5.
        // It cannot be changed at runtime.
        return new List<IMiddleware>
        {
            _tracing!,             // 1. Observability: Tracing
            _metrics!,             // 2. Observability: Metrics
            _contextGuard!,        // 3. Pre-Policy: Context Guard
            _validation!,          // 4. Pre-Policy: Validation
            _policy!,              // 5. Policy: WHYCEPOLICY™ (mandatory, single)
            _authorizationGuard!,  // 6. Post-Policy: Authorization Guard
            _idempotency!,         // 7. Post-Policy: Idempotency
            _executionGuard!       // 8. Execution: Final Gate
        };
    }

    private void ValidateMandatoryDependencies()
    {
        if (_tracing is null)
            throw new InvalidOperationException("TracingMiddleware is REQUIRED. Call UseTracing() before Build().");

        if (_metrics is null)
            throw new InvalidOperationException("MetricsMiddleware is REQUIRED. Call UseMetrics() before Build().");

        if (_contextGuard is null)
            throw new InvalidOperationException("ContextGuardMiddleware is REQUIRED. Call UseContextGuard() before Build().");

        if (_validation is null)
            throw new InvalidOperationException("ValidationMiddleware is REQUIRED. Call UseValidation() before Build().");

        if (_policy is null)
            throw new InvalidOperationException("WHYCEPOLICY middleware is REQUIRED. Call UsePolicy() before Build().");

        if (_authorizationGuard is null)
            throw new InvalidOperationException("AuthorizationGuardMiddleware is REQUIRED. Call UseAuthorizationGuard() before Build().");

        if (_idempotency is null)
            throw new InvalidOperationException("IdempotencyMiddleware is REQUIRED. Call UseIdempotency() before Build().");

        if (_executionGuard is null)
            throw new InvalidOperationException("ExecutionGuardMiddleware is REQUIRED. Call UseExecutionGuard() before Build().");
    }
}
