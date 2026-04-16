namespace Whycespace.Domain.ContentSystem.Learning.Module;

public readonly record struct ModuleId(Guid Value)
{
    public static ModuleId From(Guid value) => new(value);
    public override string ToString() => Value.ToString("D");
}
