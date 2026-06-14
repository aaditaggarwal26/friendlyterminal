import SwiftUI

struct NLCommandBarView: View {
    @Environment(SessionState.self) private var session
    @State private var inputText: String = ""
    @State private var mode: InputMode = .run
    @State private var nlState: NLState = .idle
    @FocusState private var isFocused: Bool

    enum InputMode: String, CaseIterable {
        case run = "Run"
        case ask = "Ask AI"
    }

    enum NLState: Equatable {
        case idle
        case thinking
        case suggestion(AICommandSuggestion)
        case unavailable
        case error(String)
    }

    var body: some View {
        VStack(spacing: 8) {
            if case .suggestion(let s) = nlState {
                CommandApprovalChip(
                    command: s.command,
                    explanation: s.explanation,
                    isDangerous: s.isDangerous
                ) {
                    session.executeCommand(s.command)
                    nlState = .idle
                    inputText = ""
                } onReject: {
                    nlState = .idle
                }
                .padding(.horizontal, 16)
                .transition(.move(edge: .bottom).combined(with: .opacity))
            }

            if case .unavailable = nlState {
                HStack(spacing: 8) {
                    Image(systemName: "exclamationmark.triangle")
                        .foregroundStyle(.orange)
                        .font(.system(size: 12))
                    Text("Apple Intelligence not available. Enable in System Settings → Apple Intelligence.")
                        .font(.system(size: 12))
                        .foregroundStyle(.secondary)
                    Spacer()
                    Button("Dismiss") { nlState = .idle }
                        .font(.system(size: 12))
                        .buttonStyle(.plain)
                        .foregroundStyle(.secondary)
                }
                .padding(.horizontal, 20)
                .transition(.opacity)
            }

            if case .error(let msg) = nlState {
                HStack(spacing: 8) {
                    Image(systemName: "exclamationmark.circle")
                        .foregroundStyle(.red)
                        .font(.system(size: 12))
                    Text(msg)
                        .font(.system(size: 12))
                        .foregroundStyle(.secondary)
                    Spacer()
                    Button("Dismiss") { nlState = .idle }
                        .font(.system(size: 12))
                        .buttonStyle(.plain)
                        .foregroundStyle(.secondary)
                }
                .padding(.horizontal, 20)
                .transition(.opacity)
            }

            chatInputBar
        }
        .padding(.horizontal, 16)
        .padding(.vertical, 12)
        .background(.bar)
        .animation(.easeInOut(duration: 0.2), value: nlState)
        .onAppear { isFocused = true }
    }

    private var chatInputBar: some View {
        HStack(spacing: 10) {
            modeToggle

            ZStack(alignment: .leading) {
                if inputText.isEmpty {
                    Text(mode == .run ? "Run a command…" : "Ask anything — I'll find the right command…")
                        .font(.system(size: 14, design: mode == .run ? .monospaced : .default))
                        .foregroundStyle(.tertiary)
                        .allowsHitTesting(false)
                }

                TextField("", text: $inputText, axis: .vertical)
                    .font(.system(size: 14, design: mode == .run ? .monospaced : .default))
                    .textFieldStyle(.plain)
                    .focused($isFocused)
                    .lineLimit(1...5)
                    .onSubmit { submit() }
            }

            sendButton
        }
        .padding(.horizontal, 14)
        .padding(.vertical, 10)
        .background(
            RoundedRectangle(cornerRadius: 14)
                .fill(Color(nsColor: .controlBackgroundColor))
                .shadow(color: .black.opacity(0.08), radius: 6, x: 0, y: 2)
        )
        .overlay(
            RoundedRectangle(cornerRadius: 14)
                .stroke(isFocused ? Color.accentColor.opacity(0.5) : Color(nsColor: .separatorColor).opacity(0.6), lineWidth: 1)
        )
        .animation(.easeInOut(duration: 0.15), value: isFocused)
    }

    private var modeToggle: some View {
        Menu {
            Button {
                withAnimation { mode = .run }
            } label: {
                Label("Run command", systemImage: "terminal")
            }
            Button {
                withAnimation { mode = .ask }
            } label: {
                Label("Ask AI", systemImage: "sparkles")
            }
        } label: {
            HStack(spacing: 4) {
                Image(systemName: mode == .run ? "terminal" : "sparkles")
                    .font(.system(size: 13, weight: .medium))
                    .foregroundStyle(mode == .ask ? Color.accentColor : Color.secondary)
                Image(systemName: "chevron.up.chevron.down")
                    .font(.system(size: 8, weight: .medium))
                    .foregroundStyle(.tertiary)
            }
            .padding(.horizontal, 6)
            .padding(.vertical, 4)
            .background(
                RoundedRectangle(cornerRadius: 7)
                    .fill(Color(nsColor: .quaternaryLabelColor).opacity(0.5))
            )
        }
        .buttonStyle(.plain)
        .help(mode == .run ? "Run: execute command directly" : "Ask AI: translate plain English to a command")
    }

    @ViewBuilder
    private var sendButton: some View {
        if case .thinking = nlState {
            ProgressView()
                .scaleEffect(0.7)
                .frame(width: 30, height: 30)
        } else {
            Button(action: submit) {
                Image(systemName: mode == .run ? "arrow.up" : "sparkles")
                    .font(.system(size: 13, weight: .semibold))
                    .foregroundStyle(inputText.isEmpty ? Color.secondary : Color.white)
                    .frame(width: 30, height: 30)
                    .background(
                        Circle()
                            .fill(inputText.isEmpty ? Color(nsColor: .quaternaryLabelColor) : Color.accentColor)
                    )
            }
            .buttonStyle(.plain)
            .disabled(inputText.isEmpty)
            .animation(.easeInOut(duration: 0.15), value: inputText.isEmpty)
        }
    }

    private func submit() {
        let text = inputText.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !text.isEmpty else { return }

        switch mode {
        case .run:
            session.executeCommand(text)
            inputText = ""

        case .ask:
            nlState = .thinking
            let recentCommands = session.blockStore.visibleBlocks
                .suffix(10)
                .map(\.command)
                .filter { !$0.isEmpty }

            AIManager.shared.translateToCommand(text, cwd: session.cwd, recentCommands: recentCommands) { result in
                DispatchQueue.main.async {
                    switch result {
                    case .success(let suggestion):
                        withAnimation {
                            self.nlState = .suggestion(suggestion)
                        }
                    case .failure(let error):
                        withAnimation {
                            if let aiError = error as? AIError, case .unavailable = aiError {
                                self.nlState = .unavailable
                            } else {
                                self.nlState = .error(error.localizedDescription)
                            }
                        }
                    }
                }
            }
        }
    }
}

#Preview {
    NLCommandBarView()
        .environment(SessionState())
        .frame(width: 700)
}
