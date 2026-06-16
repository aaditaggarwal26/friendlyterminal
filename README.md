# FriendlyTerminal

A friendlier macOS terminal. FriendlyTerminal wraps a real zsh shell in a native
SwiftUI app that adds command "blocks" (each command and its output grouped
together), a file sidebar and breadcrumb navigation, a built-in command help menu,
first-class support for interactive programs (vim, less, top, man, Claude Code…),
split panes, and optional on-device AI for translating plain English into commands
and explaining errors.

## Features

- **Command blocks** — each command and its output are grouped, with exit status,
  so scrollback reads like a transcript instead of a wall of text.
- **File sidebar & breadcrumbs** — browse the current directory and jump around
  the filesystem by clicking.
- **Interactive program support** — full-screen TUIs (vim, less, top, htop, man,
  nano) and raw-mode inline programs (Claude Code, REPLs) work correctly,
  including arrow keys and two-finger / scroll-wheel scrolling.
- **Command help menu** — a built-in cheat sheet of ~20 categories (Navigate,
  Files, GitHub, AI, Search, System, Network…), each listing common commands.
  Dangerous commands are flagged, and you can choose which categories to show.
- **Split panes** — open up to six terminals side by side; keyboard input follows
  the focused pane.
- **Get-started tutorial** — a dismissible in-app guide for newcomers.
- **On-device AI (optional)** — on supported hardware, translate natural language
  into shell commands and get plain-English explanations of errors, powered by
  Apple's on-device Foundation Models. No data leaves your machine.

## Requirements

- **macOS 15.0 (Sequoia) or later** to build and run the app.
- **macOS 26 (Tahoe) or later with Apple Intelligence enabled** for the optional
  AI features. The rest of the app works without them.
- **Xcode 16** (Swift 6 toolchain).
- **[XcodeGen](https://github.com/yonaskolb/XcodeGen)** — the Xcode project is
  generated from `project.yml` and is intentionally not checked into git.

## Installation

### 1. Install the build tools

XcodeGen is the only extra dependency. The easiest way is [Homebrew](https://brew.sh):

```sh
brew install xcodegen
```

SwiftTerm (the terminal emulator) is fetched automatically by Swift Package
Manager when you build — you don't need to install it yourself.

### 2. Clone the repository

```sh
git clone https://github.com/aaditaggarwal26/friendlyterminal.git
cd friendlyterminal
```

### 3. Generate the Xcode project

The `.xcodeproj` is gitignored and produced from `project.yml`, so generate it
before building (and re-run this any time `project.yml` changes):

```sh
xcodegen generate
```

### 4. Build and run

**Option A — Xcode (recommended):**

```sh
open FriendlyTerminal.xcodeproj
```

Then press **⌘R** to build and run.

**Option B — command line:**

```sh
xcodebuild \
  -project FriendlyTerminal.xcodeproj \
  -scheme FriendlyTerminal \
  -configuration Debug \
  -destination 'platform=macOS' \
  build
```

The built app lands in Xcode's DerivedData (`Build/Products/Debug/FriendlyTerminal.app`);
the exact path is printed at the end of the build. Open it with `open` or from Finder.

## Project structure

- `FriendlyTerminal/App/` — SwiftUI views and the AppKit terminal bridge.
- `FriendlyTerminal/Models/` — session, workspace, block-store, and the
  shell-integration parser.
- `FriendlyTerminal/AIKit/` — the optional on-device AI layer (gated to macOS 26).
- `FriendlyTerminal/Resources/` — bundled shell-integration script and assets.
- `project.yml` — the XcodeGen spec the `.xcodeproj` is generated from.

## How it works

FriendlyTerminal spawns a normal login zsh and sources a small shell-integration
script that emits standard OSC escape sequences (`133;A/B/C/D` for prompt/command/
output/exit, `633;E` for the command text, OSC 7 for the working directory). The
app parses those out of the PTY stream to build command blocks and track the cwd,
while the raw bytes still flow to the SwiftTerm emulator for rendering. Interactive
programs are detected from the alternate-screen (`?1049h`) and bracketed-paste
(`?2004h`) mode switches, so the UI knows when to hand the keyboard straight to the
running program.

## License

Released under the [MIT License](LICENSE).
