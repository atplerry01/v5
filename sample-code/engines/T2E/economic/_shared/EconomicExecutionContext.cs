using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Domain.Economic;

namespace Whycespace.Engines.T2E.Economic;

/// <summary>
/// Typed accessor for economic data stored in CommandContext properties.
/// Provides a clean API for downstream middleware and engines to read economic context
/// enriched by economic middleware pipeline.
/// </summary>
public sealed class EconomicExecutionContext
{
    private readonly CommandContext _context;

    public EconomicExecutionContext(CommandContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string? IdentityId => _context.Get<string>(EconomicContextKeys.IdentityId);

    /// <summary>
    /// Trust score as decimal (0–100). Replaces string-typed accessor.
    /// </summary>
    public decimal TrustScore
    {
        get
        {
            var raw = _context.Get<string>(EconomicContextKeys.TrustScore);
            return decimal.TryParse(raw, out var score) ? score : 0m;
        }
    }

    public string? FederationStatus => _context.Get<string>(EconomicContextKeys.FederationStatus);

    public string? PolicyDecision => _context.Get<string>(EconomicContextKeys.PolicyDecision);
    public string? PolicyReasonCode => _context.Get<string>(EconomicContextKeys.PolicyReasonCode);

    /// <summary>
    /// Whether the policy decision has been sealed (immutable).
    /// Once sealed, no downstream middleware may overwrite the decision.
    /// </summary>
    public bool IsPolicyDecisionSealed
    {
        get
        {
            var value = _context.Get<string>(EconomicContextKeys.PolicyDecisionSealed);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Strong-typed enforcement decision enum. Replaces boolean IsEnforcementClear.
    /// </summary>
    public EnforcementDecisionType EnforcementDecision
    {
        get
        {
            var raw = _context.Get<string>(EconomicContextKeys.EnforcementDecision);
            return Enum.TryParse<EnforcementDecisionType>(raw, ignoreCase: true, out var decision)
                ? decision
                : EnforcementDecisionType.None;
        }
    }

    public string? EnforcementReasonCode => _context.Get<string>(EconomicContextKeys.EnforcementReasonCode);

    public bool RequiresLedgerEntry
    {
        get
        {
            var value = _context.Get<string>(EconomicContextKeys.RequiresLedgerEntry);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public bool LedgerEntryRecorded
    {
        get
        {
            var value = _context.Get<string>(EconomicContextKeys.LedgerEntryRecorded);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public string? LedgerCorrelationId => _context.Get<string>(EconomicContextKeys.LedgerCorrelationId);

    public bool HasIdentity => !string.IsNullOrEmpty(IdentityId);

    public static EconomicExecutionContext From(CommandContext context) => new(context);
}
