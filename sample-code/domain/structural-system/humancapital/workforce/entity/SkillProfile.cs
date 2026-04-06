namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed class SkillProfile
{
    public Guid Id { get; }
    public string PrimarySkill { get; }

    public SkillProfile(Guid id, string primarySkill)
    {
        Id = id;
        PrimarySkill = primarySkill;
    }
}
