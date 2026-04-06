namespace Whycespace.Engines.T2E.Business.Resource.AssetResource;

public record AssetResourceCommand(
    string Action,
    string EntityId,
    object Payload
);
