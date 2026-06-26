namespace FriendlyTerminal.Core.Help;

public sealed record CommandItem(
    string Command,
    string Detail,
    bool IsDangerous = false,
    string Keywords = "");
