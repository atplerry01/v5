using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Board;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whycespace.Shared.Contracts.Operational.Sandbox.Kanban.List;

namespace Whycespace.Systems.Downstream.Operational.Sandbox.Kanban;

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
