namespace Whycespace.Domain.OperationalSystem.Sandbox.Kanban;

public static class KanbanDomainErrors
{
    public const string BoardNameRequired = "Board name is required.";
    public const string ListNameRequired = "List name is required.";
    public const string CardTitleRequired = "Card title is required.";
    public const string ListNotFound = "The specified list does not exist on this board.";
    public const string CardNotFound = "The specified card does not exist on this board.";
    public const string InvalidMove = "The card cannot be moved to the specified position.";
    public const string CardAlreadyCompleted = "The card is already completed.";
    public const string DuplicateListId = "A list with the same ID already exists on this board.";
    public const string DuplicateCardId = "A card with the same ID already exists on this board.";
    public const string DuplicateListPosition = "A list already exists at the specified position.";
    public const string InvalidPosition = "Position value must be non-negative.";
}
