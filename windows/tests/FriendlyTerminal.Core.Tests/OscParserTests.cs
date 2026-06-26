using System.Text;
using FriendlyTerminal.Core.ShellIntegration;
using Xunit;

namespace FriendlyTerminal.Core.Tests;

public class OscParserTests
{
    private static byte[] Osc(string body, bool stTerminator)
    {
        var prefix = "\x1b]" + body;
        var term = stTerminator ? "\x1b\\" : "\a";
        return Encoding.UTF8.GetBytes(prefix + term);
    }

    private static List<string> FeedInChunks(OscParser parser, byte[] bytes, int splitAt)
    {
        var seen = new List<string>();
        parser.WorkingDirectoryChanged += p => seen.Add(p);

        var take = Math.Min(splitAt, bytes.Length);
        var first = new byte[take];
        Array.Copy(bytes, 0, first, 0, take);
        parser.Feed(first);
        if (take < bytes.Length)
        {
            var rest = new byte[bytes.Length - take];
            Array.Copy(bytes, take, rest, 0, rest.Length);
            parser.Feed(rest);
        }
        return seen;
    }

    [Fact]
    public void Parses_cwd_from_9_9_with_ST_terminator()
    {
        var parser = new OscParser();
        var seen = FeedInChunks(parser, Osc("9;9;C:\\Users\\karl", stTerminator: true), 64);
        Assert.Equal(["C:\\Users\\karl"], seen);
    }

    [Fact]
    public void Parses_cwd_from_9_9_with_BEL_terminator()
    {
        var parser = new OscParser();
        parser.WorkingDirectoryChanged += p => Assert.Equal("C:\\foo", p);
        parser.Feed(Osc("9;9;C:\\foo", stTerminator: false));
    }

    [Fact]
    public void Resolves_sequence_split_across_two_chunks()
    {
        var parser = new OscParser();
        var bytes = Osc("9;9;C:\\Projects\\friendlyterminal", stTerminator: true);
        // Split right in the middle of the path.
        var seen = FeedInChunks(parser, bytes, 6);
        Assert.Equal(["C:\\Projects\\friendlyterminal"], seen);
    }

    [Fact]
    public void Plain_text_emits_nothing()
    {
        var parser = new OscParser();
        var seen = new List<string>();
        parser.WorkingDirectoryChanged += p => seen.Add(p);
        parser.Feed(Encoding.UTF8.GetBytes("PS C:\\> hello world"));
        Assert.Empty(seen);
    }

    [Fact]
    public void Unrelated_osc_does_not_fire_cwd()
    {
        var parser = new OscParser();
        var seen = new List<string>();
        parser.WorkingDirectoryChanged += p => seen.Add(p);
        parser.Feed(Osc("0;window title", stTerminator: false));
        Assert.Empty(seen);
    }

    [Fact]
    public void Osc_7_file_url_is_accepted_as_fallback()
    {
        var parser = new OscParser();
        var seen = new List<string>();
        parser.WorkingDirectoryChanged += p => seen.Add(p);
        parser.Feed(Osc("7;file:///C:/Users/karl", stTerminator: true));
        Assert.Equal(["C:\\Users\\karl"], seen);
    }
}
