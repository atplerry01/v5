namespace Whycespace.Engines.T2E.Business.Integration.CommandBridge;

public class CommandBridgeEngine
{
    private readonly CommandBridgePolicyAdapter _policy;

    public CommandBridgeEngine(CommandBridgePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CommandBridgeResult> ExecuteAsync(CommandBridgeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CommandBridgeResult(true, "Executed");
    }
}
