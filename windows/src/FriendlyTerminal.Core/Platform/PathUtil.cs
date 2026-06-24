namespace FriendlyTerminal.Core.Platform;

public static class PathUtil
{
    public static string Resolve(string path, string cwd, string home)
    {
        if (path.StartsWith('/')) return path;
        if (path.StartsWith('~')) return home + path[1..];
        return Combine(cwd, path);
    }

    public static string LastComponent(string path)
    {
        var trimmed = path.TrimEnd('/');
        var slash = trimmed.LastIndexOf('/');
        return slash < 0 ? trimmed : trimmed[(slash + 1)..];
    }

    public static string Combine(string a, string b) =>
        a.EndsWith('/') ? a + b : a + "/" + b;
}
