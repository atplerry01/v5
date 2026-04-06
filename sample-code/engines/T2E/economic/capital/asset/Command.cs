namespace Whycespace.Engines.T2E.Economic.Capital.Asset;

public record AssetCommand(string Action, string EntityId, object Payload);
public sealed record CreateAssetCommand(string Id) : AssetCommand("Create", Id, null!);
