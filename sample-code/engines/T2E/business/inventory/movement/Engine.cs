namespace Whycespace.Engines.T2E.Business.Inventory.Movement;

public class MovementEngine
{
    private readonly MovementPolicyAdapter _policy;

    public MovementEngine(MovementPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MovementResult> ExecuteAsync(MovementCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MovementResult(true, "Executed");
    }
}
