using System.Text.Json;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Opa;

public static class OpaRequestBuilder
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Build(PolicyEvaluationInput input, string policyPackage)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyPackage);

        var opaInput = new Dictionary<string, object>
        {
            ["actor"] = new Dictionary<string, object>
            {
                ["id"] = input.ActorId.ToString(),
            },
            ["action"] = input.Action,
            ["resource"] = new Dictionary<string, object>
            {
                ["type"] = input.Resource
            },
            ["environment"] = input.Environment,
            ["timestamp"] = input.Timestamp.ToString("O")
        };

        var request = new Dictionary<string, object>
        {
            ["input"] = opaInput
        };

        return JsonSerializer.Serialize(request, Options);
    }

    public static string BuildEndpoint(string policyPackage)
    {
        var path = policyPackage.Replace('.', '/');
        return $"/v1/data/{path}/decision";
    }
}
