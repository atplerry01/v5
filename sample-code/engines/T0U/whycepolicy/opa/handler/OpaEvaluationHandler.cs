using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Opa;

/// <summary>
/// Bridges PolicyEvaluationInput → OPA HTTP → PolicyEvaluationResult.
/// Stateless adapter — no business logic.
/// </summary>
public sealed class OpaEvaluationEngine
{
    private readonly OpaClient _client;
    private readonly string _policyPackage;

    public OpaEvaluationEngine(OpaClient client, string policyPackage = "whyce.policy")
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        ArgumentException.ThrowIfNullOrWhiteSpace(policyPackage);
        _policyPackage = policyPackage;
    }

    public async Task<PolicyEvaluationResult> EvaluateAsync(
        PolicyEvaluationInput input,
        CancellationToken cancellationToken = default)
    {
        var requestBody = OpaRequestBuilder.Build(input, _policyPackage);
        var endpoint = OpaRequestBuilder.BuildEndpoint(_policyPackage);

        var opaResponse = await _client.EvaluateAsync(endpoint, requestBody, cancellationToken);

        if (!opaResponse.IsSuccess)
        {
            return PolicyEvaluationResult.NonCompliant(
                opaResponse.ErrorMessage ?? "OPA evaluation failed",
                source: PolicyExecutionSource.Opa);
        }

        return OpaResponseParser.Parse(opaResponse.Body!, input);
    }

}
