using Whycespace.Engines.T0U.WhycePolicy.Evaluation;
using Whycespace.Engines.T0U.WhycePolicy.Governance;
using Whycespace.Engines.T0U.WhycePolicy.Registry;
using Whycespace.Engines.T0U.WhycePolicy.Replay;
using Whycespace.Engines.T0U.WhycePolicy.Safeguard;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T0U.WhycePolicy.Core;

/// <summary>
/// T0U WhycePolicy orchestration engine.
/// Coordinates sub-engines in canonical order:
///   Registry → Evaluation → Guard → Governance
/// Stateless — NO business logic, NO lifecycle management, orchestration only.
/// </summary>
public sealed class PolicyEngine
{
    public IPolicyRegistryEngine? Registry { get; }
    public IPolicyEvaluationEngine? Evaluation { get; }
    public ConstitutionalGuardEngine? Guard { get; }
    public IPolicyGovernanceEngine? Governance { get; }
    public PolicyReplayEngine? Replay { get; }

    public PolicyEngine(
        IPolicyRegistryEngine? registry = null,
        IPolicyEvaluationEngine? evaluation = null,
        ConstitutionalGuardEngine? guard = null,
        IPolicyGovernanceEngine? governance = null,
        PolicyReplayEngine? replay = null)
    {
        Registry = registry;
        Evaluation = evaluation;
        Guard = guard;
        Governance = governance;
        Replay = replay;
    }
}
