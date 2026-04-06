namespace Whycespace.Engines.T2E.Business.Notification.Template;

public class TemplateEngine
{
    private readonly TemplatePolicyAdapter _policy;

    public TemplateEngine(TemplatePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TemplateResult> ExecuteAsync(TemplateCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TemplateResult(true, "Executed");
    }
}
