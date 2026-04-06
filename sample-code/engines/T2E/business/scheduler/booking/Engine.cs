namespace Whycespace.Engines.T2E.Business.Scheduler.Booking;

public class BookingEngine
{
    private readonly BookingPolicyAdapter _policy;

    public BookingEngine(BookingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<BookingResult> ExecuteAsync(BookingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new BookingResult(true, "Executed");
    }
}
