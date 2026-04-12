namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public sealed class StatementLine
{
    public Guid LineId { get; }
    public Guid SourceReferenceId { get; }
    public string Description { get; }
    public decimal Amount { get; }

    public StatementLine(Guid lineId, Guid sourceReferenceId, string description, decimal amount)
    {
        if (lineId == Guid.Empty)
            throw new ArgumentException("LineId must not be empty.", nameof(lineId));

        if (sourceReferenceId == Guid.Empty)
            throw new ArgumentException("SourceReferenceId must not be empty.", nameof(sourceReferenceId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description must not be empty.", nameof(description));

        LineId = lineId;
        SourceReferenceId = sourceReferenceId;
        Description = description;
        Amount = amount;
    }
}
