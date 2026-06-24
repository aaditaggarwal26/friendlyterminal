using FriendlyTerminal.Core.Output;
using FriendlyTerminal.Core.Platform;
using Xunit;

namespace FriendlyTerminal.Core.Tests;

public class GitDetectorTests
{
    private static readonly IShellQuoter Quoter = new PowerShellQuoter();

    [Fact]
    public void Git_status_lists_changed_files_as_git_add()
    {
        var output =
            "On branch main\n" +
            "Changes not staged for commit:\n" +
            "\tmodified:   foo.cs\n" +
            "\tmodified:   bar.cs\n";

        var result = new GitStatusDetector(Quoter).Detect(output, "git status", "/x");
        var list = Assert.IsType<RenderKind.CommandList>(result);
        Assert.Equal(2, list.Items.Count);
        Assert.Equal("foo.cs", list.Items[0].Label);
        Assert.Equal("git add 'foo.cs'", list.Items[0].FollowUp);
    }

    [Fact]
    public void Git_status_short_form_is_ignored()
    {
        var result = new GitStatusDetector(Quoter).Detect("M foo.cs", "git status -s", "/x");
        Assert.Null(result);
    }

    [Fact]
    public void Git_branch_marks_current_and_offers_checkout()
    {
        var output = "* main\n  dev\n  feature/login\n";

        var result = new GitBranchDetector(Quoter).Detect(output, "git branch", "/x");
        var list = Assert.IsType<RenderKind.CommandList>(result);
        Assert.Equal(3, list.Items.Count);
        Assert.Contains("●", list.Items[0].Label);
        Assert.Equal("git checkout 'dev'", list.Items[1].FollowUp);
    }

    [Fact]
    public void Git_branch_delete_is_ignored()
    {
        Assert.Null(new GitBranchDetector(Quoter).Detect("  dev\n", "git branch -d dev", "/x"));
    }
}
