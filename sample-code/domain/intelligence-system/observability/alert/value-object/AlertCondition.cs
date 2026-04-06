namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public sealed record AlertCondition
{
    public string Operator { get; }

    private AlertCondition(string op) => Operator = op;

    public static readonly AlertCondition GreaterThan = new(">");
    public static readonly AlertCondition LessThan = new("<");
    public static readonly AlertCondition Equal = new("==");
    public static readonly AlertCondition GreaterThanOrEqual = new(">=");
    public static readonly AlertCondition LessThanOrEqual = new("<=");
    public static readonly AlertCondition NotEqual = new("!=");

    public bool Evaluate(decimal metricValue, decimal threshold) => Operator switch
    {
        ">" => metricValue > threshold,
        "<" => metricValue < threshold,
        "==" => metricValue == threshold,
        ">=" => metricValue >= threshold,
        "<=" => metricValue <= threshold,
        "!=" => metricValue != threshold,
        _ => throw new InvalidOperationException($"Unknown operator: {Operator}")
    };

    public static AlertCondition From(string op) => op switch
    {
        ">" => GreaterThan,
        "<" => LessThan,
        "==" => Equal,
        ">=" => GreaterThanOrEqual,
        "<=" => LessThanOrEqual,
        "!=" => NotEqual,
        _ => throw new ArgumentException($"Invalid alert condition operator: {op}", nameof(op))
    };

    public override string ToString() => Operator;
}
