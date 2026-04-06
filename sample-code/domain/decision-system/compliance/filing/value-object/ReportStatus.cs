namespace Whycespace.Domain.DecisionSystem.Compliance.Filing;

public sealed record ReportStatus(string Value)
{
    public static readonly ReportStatus Draft = new("DRAFT");
    public static readonly ReportStatus Submitted = new("SUBMITTED");
    public static readonly ReportStatus Accepted = new("ACCEPTED");
    public static readonly ReportStatus Rejected = new("REJECTED");
    public static readonly ReportStatus Revision = new("REVISION");

    public bool IsTerminal => this == Accepted || this == Rejected;
}
