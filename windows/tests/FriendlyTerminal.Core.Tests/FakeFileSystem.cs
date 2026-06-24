using FriendlyTerminal.Core.Platform;

namespace FriendlyTerminal.Core.Tests;

public sealed class FakeFileSystem : IFileSystem
{
    private readonly HashSet<string> _files = new();
    private readonly HashSet<string> _dirs = new();

    public string HomeDirectory { get; set; } = "/Users/test";

    public FakeFileSystem AddFile(string path) { _files.Add(path); return this; }
    public FakeFileSystem AddDir(string path) { _dirs.Add(path); return this; }

    public bool Exists(string path) => _files.Contains(path) || _dirs.Contains(path);
    public bool IsDirectory(string path) => _dirs.Contains(path);

    public IReadOnlyList<string> ListDirectory(string path) =>
        _files.Concat(_dirs).Where(p => p.StartsWith(path + "/")).ToList();

    public void MoveToTrash(string path) { }
    public void RestoreFromTrash(string trashedPath, string originalPath) { }
}
