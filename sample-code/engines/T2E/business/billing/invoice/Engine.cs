namespace Whycespace.Engines.T2E.Business.Billing.Invoice;

public class InvoiceEngine
{
    private readonly InvoicePolicyAdapter _policy;

    public InvoiceEngine(InvoicePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<InvoiceResult> ExecuteAsync(InvoiceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new InvoiceResult(true, "Executed");
    }
}
