namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public readonly record struct TemplateId
{
    public Guid Value { get; }

    public TemplateId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TemplateId value must not be empty.", nameof(value));

        Value = value;
    }
}