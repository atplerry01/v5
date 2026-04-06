using System.Text.Json;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// Validates the schema and required fields of the WhyceRequest envelope.
/// This is structural validation only — no business rules, no domain logic.
/// Rejects malformed requests before they reach downstream.
/// </summary>
public sealed class RequestValidationMiddleware : IApiMiddleware
{
    public Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var whyceRequest = DeserializeBody(request.Body);

        if (whyceRequest is null)
            return Task.FromResult(ApiResponse.BadRequest(
                "Invalid request body — expected WhyceRequest envelope", request.TraceId));

        var validationError = ValidateSchema(whyceRequest);
        if (validationError is not null)
            return Task.FromResult(ApiResponse.BadRequest(validationError, request.TraceId));

        // Validate tenant + region context resolved by TenantRegionMiddleware (via headers)
        var tenantContextError = ValidateTenantRegionHeaders(request);
        if (tenantContextError is not null)
            return Task.FromResult(ApiResponse.BadRequest(tenantContextError, request.TraceId));

        return next(request);
    }

    private static WhyceRequest? DeserializeBody(object body)
    {
        if (body is WhyceRequest typed)
            return typed;

        if (body is JsonElement json)
        {
            return JsonSerializer.Deserialize<WhyceRequest>(json.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }

    private static string? ValidateSchema(WhyceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdentityId))
            return "IdentityId is required";

        if (string.IsNullOrWhiteSpace(request.IntentType))
            return "IntentType is required";

        if (string.IsNullOrWhiteSpace(request.Intent))
            return "Intent is required";

        if (request.Payload is null)
            return "Payload is required";

        if (string.IsNullOrWhiteSpace(request.Jurisdiction))
            return "Jurisdiction is required";

        return null;
    }

    /// <summary>
    /// Validates that TenantRegionMiddleware has enriched the request with tenant + region headers.
    /// This ensures no request proceeds without explicit tenant isolation and region context.
    /// </summary>
    private static string? ValidateTenantRegionHeaders(ApiRequest request)
    {
        var tenant = TenantRegionHeaderKeys.ExtractTenant(request.Headers);
        if (tenant is null)
            return "TenantContext is required — tenant must be resolved before validation";

        var region = TenantRegionHeaderKeys.ExtractRegion(request.Headers);
        if (region is null)
            return "RegionContext is required — region must be resolved before validation";

        return null;
    }
}
