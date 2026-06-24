# FriendlyTerminal.App (WinUI 3)

The Windows app shell. **This project requires Windows to build and run** — it
uses the Windows App SDK, WebView2, and the ConPTY Win32 API. It is intentionally
excluded from `windows/FriendlyTerminal.sln` (the macOS-buildable Core solution).

## What works in this skeleton

A single window hosting an xterm.js terminal (in WebView2) wired to a real
PowerShell session over a ConPTY pseudo-console. Bytes flow both ways:

```
xterm.js  ──onData──▶ WebView2 postMessage ──▶ PtyConnection.WriteInput ──▶ ConPTY ──▶ powershell.exe
powershell.exe ──▶ ConPTY ──▶ PtyConnection.OutputReceived ──▶ ExecuteScriptAsync ──▶ term.write
```

None of the "friendly" features are here yet — those come from
`FriendlyTerminal.Core` (output detectors, undo, help catalog) in a later phase.

## Build (on Windows)

Prerequisites: Visual Studio 2022 with the **.NET Desktop** and **Windows App SDK**
workloads, or the .NET 8 SDK plus the Windows App SDK.

```powershell
cd windows\app\FriendlyTerminal.App
dotnet build -r win-x64
dotnet run -r win-x64
```

Or open `FriendlyTerminal.App.csproj` in Visual Studio and press F5.

## Files

- `App.xaml` / `MainWindow.xaml` — WinUI app + the window hosting `WebView2`.
- `MainWindow.xaml.cs` — the WebView2 ↔ ConPTY bridge.
- `Pty/PseudoConsole.cs`, `Pty/PtyConnection.cs`, `Pty/NativeMethods.cs` — the
  ConPTY pseudo-console and process plumbing.
- `Assets/terminal.html` — xterm.js front end (loads from CDN for now; vendor it
  for offline use later).

## Not yet verified

This was scaffolded on macOS and has **not been compiled on Windows**. Expect to
fix small things on the first build — package versions and the ConPTY P/Invoke
signatures are the most likely spots. Once it builds, wiring the PowerShell
profile in `windows/shell/` gives the per-command block markers.
