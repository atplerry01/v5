namespace Whycespace.Engines.T2E.Business.Inventory.Reservation;

public class ReservationEngine
{
    private readonly ReservationPolicyAdapter _policy;

    public ReservationEngine(ReservationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ReservationResult> ExecuteAsync(ReservationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ReservationResult(true, "Executed");
    }
}
