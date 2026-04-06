namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public sealed record TemplateType(string Value)
{
    public static readonly TemplateType Transactional = new("transactional");
    public static readonly TemplateType Marketing = new("marketing");
    public static readonly TemplateType System = new("system");
    public static readonly TemplateType Alert = new("alert");
}
