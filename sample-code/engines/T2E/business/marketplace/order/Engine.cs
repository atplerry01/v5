namespace Whycespace.Engines.T2E.Business.Marketplace.Order;

public class OrderEngine
{
    private readonly OrderPolicyAdapter _policy;

    public OrderEngine(OrderPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<OrderResult> ExecuteAsync(OrderCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new OrderResult(true, "Executed");
    }
}
