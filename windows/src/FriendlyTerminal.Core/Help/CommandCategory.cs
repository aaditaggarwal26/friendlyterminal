namespace FriendlyTerminal.Core.Help;

public sealed record CommandCategory(
    string Id,
    string Name,
    string Icon,
    IReadOnlyList<CommandItem> Commands);
