using FriendlyTerminal.Core.Platform;

namespace FriendlyTerminal.Core.Output;

public sealed class GitStatusDetector : IOutputDetector
{
    private static readonly string[] StatusKeywords =
    {
        "modified:", "new file:", "deleted:", "renamed:", "copied:",
        "typechange:", "both modified:", "both added:", "added by us:",
        "deleted by us:", "added by them:", "deleted by them:", "unmerged:"
    };

    private readonly IShellQuoter _quoter;

    public GitStatusDetector(IShellQuoter quoter) => _quoter = quoter;

    public RenderKind? Detect(string output, string command, string cwd)
    {
        var trimmed = command.Trim();
        if (trimmed != "git status" && !trimmed.StartsWith("git status ")) return null;
        if (trimmed.Contains("-s") || trimmed.Contains("--short") || trimmed.Contains("--porcelain")) return null;

        var items = new List<CommandListItem>();
        var seen = new HashSet<string>();

        foreach (var rawLine in output.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            string? path = null;
            var keyword = Array.Find(StatusKeywords, k => line.StartsWith(k));
            if (keyword != null)
                path = line[keyword.Length..].Trim();
            else if (rawLine.StartsWith('\t') && !line.StartsWith('('))
                path = line;

            if (string.IsNullOrEmpty(path)) continue;

            var arrow = path.IndexOf("-> ", StringComparison.Ordinal);
            if (arrow >= 0) path = path[(arrow + 3)..].Trim();
            if (path.Length == 0 || !seen.Add(path)) continue;

            items.Add(new CommandListItem(
                Label: path,
                Detail: "Stage this file (git add)",
                Icon: "git-add",
                FollowUp: $"git add {_quoter.Quote(path)}"));
        }

        if (items.Count == 0) return null;
        return new RenderKind.CommandList(
            "Click a file to fill in “git add” for it — then press Return to stage it.",
            items);
    }
}
