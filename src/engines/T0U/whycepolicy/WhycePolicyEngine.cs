using System.Security.Cryptography;
using System.Text;
using Whyce.Shared.Contracts.Engine;

namespace Whyce.Engines.T0U.WhycePolicy;

public sealed class WhycePolicyEngine
{
    public Task<PolicyDecision> Handle(WhycePolicyCommand command, IEngineContext context)
    {
        if (string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new PolicyDecision(
                IsAllowed: false,
                DecisionHash: ComputeDecisionHash(command)));
        }

        if (command.TrustScore < 10)
        {
            return Task.FromResult(new PolicyDecision(
                IsAllowed: false,
                DecisionHash: ComputeDecisionHash(command)));
        }

        return Task.FromResult(new PolicyDecision(
            IsAllowed: true,
            DecisionHash: ComputeDecisionHash(command)));
    }

    private static string ComputeDecisionHash(WhycePolicyCommand command)
    {
        var sortedRoles = string.Join(",", command.Roles.OrderBy(r => r, StringComparer.Ordinal));
        var input = $"{command.PolicyName}{command.IdentityId}{sortedRoles}{command.TrustScore}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
