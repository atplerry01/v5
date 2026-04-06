namespace Whycespace.Engines.T3I.Reporting;

public sealed class ReportEngine
{
    public ReportResult Generate(ReportCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new ReportResult(command.ReportId, true, string.Empty);
    }
}

public sealed record ReportCommand(string ReportId, string ReportType, DateTimeOffset From, DateTimeOffset To);

public sealed record ReportResult(string ReportId, bool Success, string Content);
