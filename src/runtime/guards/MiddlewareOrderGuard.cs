using Whyce.Runtime.Middleware;
using Whyce.Runtime.Middleware.Execution;
using Whyce.Runtime.Middleware.Observability;
using Whyce.Runtime.Middleware.PostPolicy;
using Whyce.Runtime.Middleware.PrePolicy;

namespace Whyce.Runtime.Guards;

/// <summary>
/// Middleware Order Guard — validates that the middleware pipeline
/// follows the canonical LOCKED order per WBSM v3.5.
///
/// LOCKED ORDER:
///   1. TracingMiddleware
///   2. MetricsMiddleware
///   3. ContextGuardMiddleware
///   4. ValidationMiddleware
///   5. PolicyMiddleware
///   6. AuthorizationGuardMiddleware
///   7. IdempotencyMiddleware
///   8. ExecutionGuardMiddleware
///
/// Any deviation is a violation.
/// </summary>
public static class MiddlewareOrderGuard
{
    private static readonly Type[] CanonicalOrder =
    [
        typeof(TracingMiddleware),
        typeof(Whyce.Runtime.Middleware.Observability.MetricsMiddleware),
        typeof(ContextGuardMiddleware),
        typeof(ValidationMiddleware),
        typeof(Whyce.Runtime.Middleware.Policy.PolicyMiddleware),
        typeof(AuthorizationGuardMiddleware),
        typeof(IdempotencyMiddleware),
        typeof(ExecutionGuardMiddleware)
    ];

    /// <summary>
    /// Validates the middleware pipeline order against the canonical order.
    /// Returns a list of violations found.
    /// </summary>
    public static IReadOnlyList<string> Validate(IReadOnlyList<IMiddleware> middlewares)
    {
        var violations = new List<string>();

        if (middlewares.Count != CanonicalOrder.Length)
        {
            violations.Add(
                $"Expected {CanonicalOrder.Length} middleware, found {middlewares.Count}.");
        }

        var count = Math.Min(middlewares.Count, CanonicalOrder.Length);
        for (var i = 0; i < count; i++)
        {
            var actual = middlewares[i].GetType();
            var expected = CanonicalOrder[i];

            if (actual != expected)
            {
                violations.Add(
                    $"Position {i}: expected {expected.Name}, found {actual.Name}.");
            }
        }

        return violations;
    }
}
