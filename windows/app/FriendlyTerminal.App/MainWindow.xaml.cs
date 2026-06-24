using System.IO;
using FriendlyTerminal.App.Pty;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;

namespace FriendlyTerminal.App;

public sealed partial class MainWindow : Window
{
    private PtyConnection? _pty;

    public MainWindow()
    {
        InitializeComponent();
        Terminal.Loaded += OnTerminalLoaded;
        Closed += (_, _) => _pty?.Dispose();
    }

    private async void OnTerminalLoaded(object sender, RoutedEventArgs e)
    {
        await Terminal.EnsureCoreWebView2Async();
        Terminal.CoreWebView2.WebMessageReceived += OnWebMessage;

        var html = Path.Combine(AppContext.BaseDirectory, "Assets", "terminal.html");
        Terminal.CoreWebView2.Navigate(new Uri(html).AbsoluteUri);
    }

    private void OnWebMessage(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var message = args.TryGetWebMessageAsString();
        if (string.IsNullOrEmpty(message)) return;

        if (message == "ready")
            StartShell();
        else if (message.StartsWith("i:"))
            _pty?.WriteInput(message[2..]);
    }

    private void StartShell()
    {
        _pty = new PtyConnection("powershell.exe", 120, 30);
        _pty.OutputReceived += OnPtyOutput;
        _pty.Start();
    }

    private void OnPtyOutput(byte[] data)
    {
        var b64 = Convert.ToBase64String(data);
        DispatcherQueue.TryEnqueue(async () =>
        {
            await Terminal.CoreWebView2.ExecuteScriptAsync($"window.ptyWrite('{b64}')");
        });
    }
}
