namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public readonly record struct EnforcementAction
{
    public Guid DecisionReference { get; }
    public string ActionType { get; }

    public EnforcementAction(Guid decisionReference, string actionType)
    {
        if (decisionReference == Guid.Empty)
            throw new ArgumentException("DecisionReference must not be empty.", nameof(decisionReference));

        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("ActionType must not be empty.", nameof(actionType));

        DecisionReference = decisionReference;
        ActionType = actionType;
    }
}
