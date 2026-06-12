import SwiftUI

@main
struct FriendlyTerminalApp: App {
    @State private var sessionState = SessionState()

    var body: some Scene {
        WindowGroup {
            MainWindowView()
                .environment(sessionState)
        }
        .windowStyle(.titleBar)
        .windowToolbarStyle(.unified)
        .defaultSize(width: 1100, height: 720)
        .commands {
            FriendlyTerminalCommands()
        }
    }
}
