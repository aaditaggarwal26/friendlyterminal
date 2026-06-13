import SwiftUI

struct MainWindowView: View {
    @Environment(SessionState.self) private var session

    var body: some View {
        VStack(spacing: 0) {
            BreadcrumbBarView()

            Divider()

            HSplitView {
                if session.sidebarVisible {
                    FileSidebarView()
                        .frame(minWidth: 180, idealWidth: 220, maxWidth: 280)
                }

                VStack(spacing: 0) {
                    TerminalContainerView()
                        .frame(maxWidth: .infinity, maxHeight: .infinity)

                    NLCommandBarView()
                }
                .frame(maxWidth: .infinity, maxHeight: .infinity)
            }
        }
        .frame(minWidth: 700, minHeight: 400)
        .background(Color(nsColor: .textBackgroundColor))
        .onAppear {
            session.refreshFileItems()
        }
    }
}

#Preview {
    MainWindowView()
        .environment(SessionState())
        .frame(width: 1100, height: 720)
}
