namespace Whycespace.Engines.T2E.Operational.Mobility;

public abstract record MobilityCommand;

public sealed record CreateMobilityCommand(string Id) : MobilityCommand;
