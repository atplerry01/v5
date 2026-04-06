namespace Whycespace.Domain.DecisionSystem.Compliance.Regulation;

public sealed record RegulationType(string Value)
{
    public static readonly RegulationType Financial = new("FINANCIAL");
    public static readonly RegulationType DataPrivacy = new("DATA_PRIVACY");
    public static readonly RegulationType AmlCft = new("AML_CFT");
    public static readonly RegulationType Securities = new("SECURITIES");
    public static readonly RegulationType Tax = new("TAX");
    public static readonly RegulationType Environmental = new("ENVIRONMENTAL");
    public static readonly RegulationType Labor = new("LABOR");
}
