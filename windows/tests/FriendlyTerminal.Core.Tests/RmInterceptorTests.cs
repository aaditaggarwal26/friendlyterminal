using FriendlyTerminal.Core.Undo;
using Xunit;

namespace FriendlyTerminal.Core.Tests;

public class RmInterceptorTests
{
    private const string Cwd = "/Users/test/project";

    [Fact]
    public void Recognizes_safe_rm_of_existing_targets()
    {
        var fs = new FakeFileSystem().AddDir("/Users/test/project/build");
        var targets = new RmInterceptor(fs).SafeTargets("rm -rf build", Cwd);
        Assert.Equal(new[] { "/Users/test/project/build" }, targets);
    }

    [Fact]
    public void Rejects_globs_and_metacharacters()
    {
        var fs = new FakeFileSystem();
        Assert.Null(new RmInterceptor(fs).SafeTargets("rm *.txt", Cwd));
    }

    [Fact]
    public void Rejects_missing_targets()
    {
        var fs = new FakeFileSystem();
        Assert.Null(new RmInterceptor(fs).SafeTargets("rm gone.txt", Cwd));
    }

    [Fact]
    public void Rejects_unknown_flags()
    {
        var fs = new FakeFileSystem().AddFile("/Users/test/project/a.txt");
        Assert.Null(new RmInterceptor(fs).SafeTargets("rm -z a.txt", Cwd));
    }
}
