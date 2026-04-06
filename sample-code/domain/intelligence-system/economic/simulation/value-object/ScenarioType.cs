namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed record ScenarioType(string Value)
{
    public static readonly ScenarioType StressTest = new("stress_test");
    public static readonly ScenarioType WhatIf = new("what_if");
    public static readonly ScenarioType MonteCarlo = new("monte_carlo");
    public static readonly ScenarioType Sensitivity = new("sensitivity");
}
