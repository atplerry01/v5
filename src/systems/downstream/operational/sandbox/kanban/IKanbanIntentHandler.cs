using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;

namespace Whyce.Systems.Downstream.Operational.Sandbox.Kanban;

public interface IKanbanIntentHandler
{
    Task<KanbanSystemResult> HandleCreateBoardAsync(CreateKanbanBoardIntent intent);
    Task<KanbanSystemResult> HandleCreateListAsync(CreateKanbanListIntent intent);
    Task<KanbanSystemResult> HandleCreateCardAsync(CreateKanbanCardIntent intent);
    Task<KanbanSystemResult> HandleMoveCardAsync(MoveKanbanCardIntent intent);
    Task<KanbanSystemResult> HandleReorderCardAsync(ReorderKanbanCardIntent intent);
    Task<KanbanSystemResult> HandleCompleteCardAsync(CompleteKanbanCardIntent intent);
    Task<KanbanSystemResult> HandleUpdateCardAsync(UpdateKanbanCardIntent intent);
}
