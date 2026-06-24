namespace FriendlyTerminal.Core.Platform;

public sealed class PowerShellQuoter : IShellQuoter
{
    public string Quote(string argument) =>
        "'" + argument.Replace("'", "''") + "'";
}
