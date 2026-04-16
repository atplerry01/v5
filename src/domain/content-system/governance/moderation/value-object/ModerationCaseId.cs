namespace Whycespace.Domain.ContentSystem.Governance.Moderation;

public readonly record struct ModerationCaseId(Guid Value)
{
    public static ModerationCaseId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
