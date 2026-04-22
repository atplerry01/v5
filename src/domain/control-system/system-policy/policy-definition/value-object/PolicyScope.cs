namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

public sealed record PolicyScope
{
    public string Classification { get; }
    public string? Context { get; }
    public string ActionMask { get; }

    public PolicyScope(string classification, string actionMask, string? context = null)
    {
        if (string.IsNullOrEmpty(classification))
            throw PolicyDefinitionErrors.ClassificationMustNotBeEmpty();

        if (string.IsNullOrEmpty(actionMask))
            throw PolicyDefinitionErrors.ActionMaskMustNotBeEmpty();

        Classification = classification;
        Context = context;
        ActionMask = actionMask;
    }

    public bool IsClassificationWide => Context is null;

    public override string ToString() =>
        Context is null ? $"{Classification}::{ActionMask}" : $"{Classification}/{Context}::{ActionMask}";
}
