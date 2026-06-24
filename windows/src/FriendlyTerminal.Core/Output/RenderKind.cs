namespace FriendlyTerminal.Core.Output;

public abstract record RenderKind
{
    public sealed record CommandList(string Hint, IReadOnlyList<CommandListItem> Items) : RenderKind;
}

public sealed record CommandListItem(string Label, string? Detail, string Icon, string FollowUp);
