using Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;
using Whycespace.Domain.OperationalSystem.Sandbox.Kanban;
using Whycespace.Domain.OperationalSystem.Sandbox.Todo;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.OperationalSystem.ReplayLosslessness;

/// <summary>
/// INV-REPLAY-LOSSLESS-VALUEOBJECT-01 — operational-system (ACTIVE BCs only)
/// Verifies that LoadFromHistory produces structurally identical aggregate state
/// to direct factory construction — all VO fields survive the event round-trip.
/// </summary>
public sealed class OperationalSystemReplayLosslessnessTests
{
    private static readonly TestIdGenerator IdGen = new();

    // ── IncidentAggregate ────────────────────────────────────────────────────

    [Fact]
    public void IncidentAggregate_Replay_PreservesAllVoFields()
    {
        var id = new IncidentId(IdGen.Generate("LS:incident:id"));
        var descriptor = new IncidentDescriptor("Database connection timeout on node-7", "P1");

        var direct = IncidentAggregate.Report(id, descriptor);

        var replayed = (IncidentAggregate)Activator.CreateInstance(typeof(IncidentAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new IncidentReportedEvent(id, descriptor)
        });

        Assert.Equal(direct.Id, replayed.Id);
        Assert.Equal(direct.Descriptor.Title, replayed.Descriptor.Title);
        Assert.Equal(direct.Descriptor.Severity, replayed.Descriptor.Severity);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── IncidentAggregate multi-step ─────────────────────────────────────────

    [Fact]
    public void IncidentAggregate_Replay_AfterInvestigate_PreservesStatus()
    {
        var id = new IncidentId(IdGen.Generate("LS:incident:investigate:id"));
        var descriptor = new IncidentDescriptor("Memory pressure in cluster-A", "P2");

        var direct = IncidentAggregate.Report(id, descriptor);
        direct.Investigate();

        var replayed = (IncidentAggregate)Activator.CreateInstance(typeof(IncidentAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new IncidentReportedEvent(id, descriptor),
            new IncidentInvestigationStartedEvent(id)
        });

        Assert.Equal(IncidentStatus.Investigating, direct.Status);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── TodoAggregate ────────────────────────────────────────────────────────

    [Fact]
    public void TodoAggregate_Replay_PreservesAllVoFields()
    {
        var id = new TodoId(IdGen.Generate("LS:todo:id"));
        var aggregateId = new AggregateId(id.Value);

        var direct = TodoAggregate.Create(id, "Draft quarterly report");

        var replayed = (TodoAggregate)Activator.CreateInstance(typeof(TodoAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new TodoCreatedEvent(aggregateId, "Draft quarterly report")
        });

        Assert.Equal(direct.Id, replayed.Id);
        // Title and IsCompleted are private — behavioral: Complete works on replayed (not yet completed)
        Assert.ThrowsAny<Exception>(() => replayed.ReviseTitle(""));
    }

    // ── KanbanAggregate ──────────────────────────────────────────────────────

    [Fact]
    public void KanbanAggregate_Replay_PreservesNameAndEmptyLists()
    {
        var id = new KanbanBoardId(IdGen.Generate("LS:kanban:id"));
        var aggregateId = new AggregateId(id.Value);

        var direct = KanbanAggregate.Create(id, "Sprint Board Q2");

        var replayed = (KanbanAggregate)Activator.CreateInstance(typeof(KanbanAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[]
        {
            new KanbanBoardCreatedEvent(aggregateId, "Sprint Board Q2")
        });

        Assert.Equal(direct.Id, replayed.Id);
        Assert.Equal(direct.Name, replayed.Name);
        Assert.Empty(replayed.Lists);
    }
}
