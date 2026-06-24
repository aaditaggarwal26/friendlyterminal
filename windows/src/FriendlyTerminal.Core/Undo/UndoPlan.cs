namespace FriendlyTerminal.Core.Undo;

public sealed record UndoPlan(string Label, IReadOnlyList<UndoAction> Actions);

public abstract record UndoAction
{
    public sealed record Shell(string Command) : UndoAction;
    public sealed record Trash(string Path) : UndoAction;
    public sealed record Restore(string TrashedPath, string OriginalPath) : UndoAction;
}
