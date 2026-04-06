using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Context.Economic;

/// <summary>
/// Links economic execution to policy decisions and chain evidence (E6).
/// Attaches the DecisionHash to the command context so that:
///   - Ledger entries reference the policy decision
///   - Chain blocks carry the economic evidence trail
/// </summary>
public static class EconomicChainLinker
{
    public const string DecisionHashKey = "Economic.DecisionHash";
    public const string EconomicClassificationKey = "Economic.Classification";
    public const string EconomicDomainKey = "Economic.Domain";
    public const string EconomicOperationKey = "Economic.Operation";

    public static void LinkDecision(CommandContext context, string decisionHash)
    {
        context.Set(DecisionHashKey, decisionHash);
    }

    public static string? GetLinkedDecisionHash(CommandContext context)
    {
        return context.Get<string>(DecisionHashKey);
    }

    public static void TagEconomicCommand(CommandContext context, string domain, string operation)
    {
        context.Set(EconomicClassificationKey, "economic");
        context.Set(EconomicDomainKey, domain);
        context.Set(EconomicOperationKey, operation);
    }

    public static bool IsEconomicCommand(CommandContext context)
    {
        return context.Get<string>(EconomicClassificationKey) == "economic";
    }
}
