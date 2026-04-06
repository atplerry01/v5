using Whycespace.Shared.Contracts.Domain.Structural;

namespace Whycespace.Engines.T2E.Structural.Cluster.CrossSpv;

public record CrossSpvCommand(string Action, string EntityId, object Payload);

public sealed record CreateCrossSpvTransactionCommand(
    string Id,
    string RootSpvId,
    IReadOnlyList<SpvLegDto> Legs) : CrossSpvCommand("Create", Id, null!);

public sealed record PrepareCrossSpvTransactionCommand(
    string Id,
    string TransactionId) : CrossSpvCommand("Prepare", Id, null!);

public sealed record CommitCrossSpvTransactionCommand(
    string Id,
    string TransactionId) : CrossSpvCommand("Commit", Id, null!);

public sealed record FailCrossSpvTransactionCommand(
    string Id,
    string TransactionId,
    string Reason) : CrossSpvCommand("Fail", Id, null!);
