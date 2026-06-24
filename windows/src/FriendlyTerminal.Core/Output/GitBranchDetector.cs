using FriendlyTerminal.Core.Platform;

namespace FriendlyTerminal.Core.Output;

public sealed class GitBranchDetector : IOutputDetector
{
    private readonly IShellQuoter _quoter;

    public GitBranchDetector(IShellQuoter quoter) => _quoter = quoter;

    public RenderKind? Detect(string output, string command, string cwd)
    {
        var trimmed = command.Trim();
        if (trimmed != "git branch" && !trimmed.StartsWith("git branch ")) return null;

        var rest = trimmed["git branch".Length..].Trim();
        if (rest.Length > 0)
        {
            foreach (var arg in rest.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!arg.StartsWith('-')) return null;
                if (arg.Contains('d') || arg.Contains('D') || arg.Contains('m')) return null;
            }
        }

        var items = new List<CommandListItem>();
        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            var isCurrent = line.StartsWith("* ");
            if (isCurrent || line.StartsWith("+ "))
                line = line[2..].Trim();

            var name = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? line;
            if (name.Length == 0 || name.StartsWith('(') || name.Contains("->")) continue;

            items.Add(new CommandListItem(
                Label: isCurrent ? $"{name}  ●" : name,
                Detail: isCurrent ? "Current branch" : "Switch to this branch",
                Icon: "git-branch",
                FollowUp: $"git checkout {_quoter.Quote(name)}"));
        }

        if (items.Count == 0) return null;
        return new RenderKind.CommandList(
            "Click a branch to fill in “git checkout” — then press Return to switch to it.",
            items);
    }
}
