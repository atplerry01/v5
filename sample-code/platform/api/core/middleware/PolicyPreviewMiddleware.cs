using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// Policy preview pipeline placeholder.
/// The actual policy preview is performed in the IntentGatewayController
/// AFTER classification and routing, where we have full context
/// (ClassifiedIntent + IntentRoute + WhyceIdentity).
///
/// This middleware exists to maintain pipeline position consistency.
/// It is a pass-through — it does NOT block, does NOT evaluate policy.
///
/// E19.5 RULE: PolicyPreview is ADVISORY ONLY — never blocks execution.
/// </summary>
public sealed class PolicyPreviewMiddleware : IApiMiddleware
{
    public PolicyPreviewMiddleware()
    {
    }

    public Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        // Pass-through — policy preview happens in controller with full context
        return next(request);
    }
}
