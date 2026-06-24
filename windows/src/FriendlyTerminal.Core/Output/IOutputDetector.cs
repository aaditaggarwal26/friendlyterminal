namespace FriendlyTerminal.Core.Output;

public interface IOutputDetector
{
    RenderKind? Detect(string output, string command, string cwd);
}
