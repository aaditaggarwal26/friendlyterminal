using FriendlyTerminal.Core.Platform;

namespace FriendlyTerminal.Core.Undo;

public sealed class RmInterceptor
{
    private const string UnsafeChars = "'\"*?[]{}|&;<>$`~";
    private static readonly HashSet<char> AllowedFlags = new("rfRdiv");

    private readonly IFileSystem _fs;

    public RmInterceptor(IFileSystem fs) => _fs = fs;

    public IReadOnlyList<string>? SafeTargets(string command, string cwd)
    {
        var trimmed = command.Trim();
        if (trimmed.IndexOfAny(UnsafeChars.ToCharArray()) >= 0) return null;

        var parts = trimmed.Split(' ', '\t').Where(p => p.Length > 0).ToArray();
        if (parts.Length == 0 || parts[0] != "rm") return null;

        var targets = new List<string>();
        foreach (var arg in parts.Skip(1))
        {
            if (arg.StartsWith('-'))
            {
                if (!arg.Skip(1).All(AllowedFlags.Contains)) return null;
                continue;
            }
            var path = PathUtil.Resolve(arg, cwd, _fs.HomeDirectory);
            if (!_fs.Exists(path)) return null;
            targets.Add(path);
        }

        return targets.Count == 0 ? null : targets;
    }
}
