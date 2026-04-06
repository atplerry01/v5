namespace Whycespace.Engines.T2E.Operational.Property.PropertyLetting.SME;

public abstract record SmeCommand;

public sealed record CreateSmeCommand(string Id) : SmeCommand;
