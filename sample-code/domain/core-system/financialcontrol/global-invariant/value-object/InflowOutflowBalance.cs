namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

/// <summary>
/// Tracks cumulative inflow and outflow across the system.
/// Invariant: TotalInflow must always equal TotalOutflow at settlement.
/// </summary>
public sealed record InflowOutflowBalance
{
    public decimal TotalInflow { get; }
    public decimal TotalOutflow { get; }
    public decimal Variance => TotalInflow - TotalOutflow;
    public bool IsBalanced => Variance == 0m;

    private InflowOutflowBalance(decimal totalInflow, decimal totalOutflow)
    {
        TotalInflow = totalInflow;
        TotalOutflow = totalOutflow;
    }

    public static InflowOutflowBalance Initial() => new(0m, 0m);

    public static InflowOutflowBalance From(decimal totalInflow, decimal totalOutflow) =>
        totalInflow < 0m || totalOutflow < 0m
            ? throw new ArgumentOutOfRangeException("Inflow and outflow must be non-negative.")
            : new(totalInflow, totalOutflow);

    public InflowOutflowBalance RecordInflow(decimal amount) =>
        amount <= 0m
            ? throw new ArgumentOutOfRangeException(nameof(amount), "Inflow amount must be positive.")
            : new(TotalInflow + amount, TotalOutflow);

    public InflowOutflowBalance RecordOutflow(decimal amount) =>
        amount <= 0m
            ? throw new ArgumentOutOfRangeException(nameof(amount), "Outflow amount must be positive.")
            : new(TotalInflow, TotalOutflow + amount);
}
