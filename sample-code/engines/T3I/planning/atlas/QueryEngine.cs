namespace Whycespace.Engines.T3I.Atlas;

public sealed class QueryEngine
{
    public QueryResult Query(QueryCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return new QueryResult(command.QueryId, true, []);
    }
}

public sealed record QueryCommand(string QueryId, string Expression);

public sealed record QueryResult(string QueryId, bool Success, IReadOnlyList<string> Results);
