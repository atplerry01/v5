namespace Whycespace.Engines.T2E.Business.Notification.Channel;

public class ChannelEngine
{
    private readonly ChannelPolicyAdapter _policy;

    public ChannelEngine(ChannelPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ChannelResult> ExecuteAsync(ChannelCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ChannelResult(true, "Executed");
    }
}
