namespace Whycespace.Domain.BusinessSystem.Notification.Template;

public readonly record struct TemplateContent
{
    public string Subject { get; }
    public string Body { get; }

    public TemplateContent(string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Template subject must not be empty.", nameof(subject));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Template body must not be empty.", nameof(body));

        Subject = subject;
        Body = body;
    }
}