using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Module;

public sealed record ModuleTitle : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }
    private ModuleTitle(string v) => Value = v;
    public static ModuleTitle Create(string v)
    {
        if (string.IsNullOrWhiteSpace(v)) throw ModuleErrors.InvalidTitle();
        var t = v.Trim();
        if (t.Length > MaxLength) throw ModuleErrors.TitleTooLong(MaxLength);
        return new ModuleTitle(t);
    }
    public override string ToString() => Value;
}
