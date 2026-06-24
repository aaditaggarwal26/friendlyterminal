namespace FriendlyTerminal.Core.Platform;

public sealed class PosixShellQuoter : IShellQuoter
{
    public string Quote(string argument) =>
        "'" + argument.Replace("'", "'\\''") + "'";
}
