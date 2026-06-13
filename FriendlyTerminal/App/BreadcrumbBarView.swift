import SwiftUI

struct BreadcrumbBarView: View {
    @Environment(SessionState.self) private var session

    var body: some View {
        ScrollViewReader { proxy in
            ScrollView(.horizontal, showsIndicators: false) {
                HStack(spacing: 2) {
                    Button {
                        withAnimation(.easeInOut(duration: 0.2)) {
                            session.sidebarVisible.toggle()
                        }
                    } label: {
                        Image(systemName: "sidebar.left")
                            .foregroundStyle(.secondary)
                    }
                    .buttonStyle(.plain)
                    .help("Toggle sidebar")
                    .padding(.trailing, 6)

                    ForEach(Array(session.breadcrumbs.enumerated()), id: \.element.id) { index, crumb in
                        HStack(spacing: 2) {
                            if index > 0 {
                                Image(systemName: "chevron.right")
                                    .font(.system(size: 10, weight: .medium))
                                    .foregroundStyle(.tertiary)
                            }

                            Button(crumb.name) {
                                session.navigateShellTo(crumb.path)
                            }
                            .buttonStyle(.plain)
                            .font(.system(size: 12, weight: index == session.breadcrumbs.count - 1 ? .semibold : .regular))
                            .foregroundStyle(index == session.breadcrumbs.count - 1 ? .primary : .secondary)
                            .id(crumb.id)
                        }
                    }

                    Spacer()

                    Button {
                        session.refreshFileItems()
                    } label: {
                        Image(systemName: "arrow.clockwise")
                            .foregroundStyle(.secondary)
                    }
                    .buttonStyle(.plain)
                    .help("Refresh")
                }
                .padding(.horizontal, 12)
                .padding(.vertical, 7)
            }
            .onChange(of: session.breadcrumbs.last?.id) { _, newID in
                if let id = newID {
                    withAnimation { proxy.scrollTo(id, anchor: .trailing) }
                }
            }
        }
        .background(.bar)
    }
}

#Preview {
    BreadcrumbBarView()
        .environment(SessionState())
        .frame(width: 600)
}
