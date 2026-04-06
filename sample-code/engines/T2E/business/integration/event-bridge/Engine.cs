namespace Whycespace.Engines.T2E.Business.Integration.EventBridge;

public class EventBridgeEngine
{
    private readonly EventBridgePolicyAdapter _policy;

    public EventBridgeEngine(EventBridgePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EventBridgeResult> ExecuteAsync(EventBridgeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EventBridgeResult(true, "Executed");
    }
}
