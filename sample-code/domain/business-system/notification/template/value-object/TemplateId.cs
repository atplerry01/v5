using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public readonly record struct TemplateId(Guid Value)
{
    public static TemplateId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly TemplateId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(TemplateId id) => id.Value;
    public static implicit operator TemplateId(Guid id) => new(id);
}
