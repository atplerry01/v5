using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public sealed record DispatchCondition
{
    public DispatchConditionType ConditionType { get; }
    public string MatchValue { get; }

    public DispatchCondition(DispatchConditionType conditionType, string matchValue)
    {
        Guard.Against(
            conditionType == DispatchConditionType.AlwaysMatch && !string.IsNullOrEmpty(matchValue),
            "AlwaysMatch condition must have an empty MatchValue.");

        ConditionType = conditionType;
        MatchValue = matchValue ?? string.Empty;
    }
}
