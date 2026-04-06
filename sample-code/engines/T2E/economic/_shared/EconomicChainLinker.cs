using Whycespace.Runtime.Command;

namespace Whycespace.Engines.T2E.Economic;

/// <summary>
/// Links economic execution to policy decisions and chain evidence (E6).
/// Attaches the DecisionHash to the command context so that:
///   - Ledger entries reference the policy decision
///   - Chain blocks carry the economic evidence trail
///
/// Flow: Economic Command → PolicyDecision → DecisionHash → Execution
///       → Ledger Entry → Link DecisionHash → Anchor
/// </summary>
public static class EconomicChainLinker
{
    public const string DecisionHashKey = "Economic.DecisionHash";
    public const string EconomicClassificationKey = "Economic.Classification";
    public const string EconomicDomainKey = "Economic.Domain";
    public const string EconomicOperationKey = "Economic.Operation";

    /// <summary>
    /// Links a decision hash to the economic execution context.
    /// Called after policy evaluation, before engine execution.
    /// </summary>
    public static void LinkDecision(CommandContext context, string decisionHash)
    {
        context.Set(DecisionHashKey, decisionHash);
    }

    /// <summary>
    /// Reads the linked decision hash from the execution context.
    /// </summary>
    public static string? GetLinkedDecisionHash(CommandContext context)
    {
        return context.Get<string>(DecisionHashKey);
    }

    /// <summary>
    /// Tags the command with economic classification metadata.
    /// </summary>
    public static void TagEconomicCommand(CommandContext context, string domain, string operation)
    {
        context.Set(EconomicClassificationKey, "economic");
        context.Set(EconomicDomainKey, domain);
        context.Set(EconomicOperationKey, operation);
    }

    /// <summary>
    /// Checks if the command is tagged as economic.
    /// </summary>
    public static bool IsEconomicCommand(CommandContext context)
    {
        return context.Get<string>(EconomicClassificationKey) == "economic";
    }
}
