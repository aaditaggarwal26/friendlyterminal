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
        VStack(spacing: 0) {
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
                .padding(.horizontal, 12)
                .padding(.top, 8)
                .transition(.move(edge: .bottom).combined(with: .opacity))
            }

            if case .unavailable = nlState {
                HStack {
                    Image(systemName: "exclamationmark.triangle")
                        .foregroundStyle(.orange)
                    Text("Apple Intelligence not available. Enable in System Settings → Apple Intelligence.")
                        .font(.system(size: 11))
                        .foregroundStyle(.secondary)
                    Spacer()
                    Button("Dismiss") { nlState = .idle }
                        .font(.system(size: 11))
                }
                .padding(.horizontal, 12)
                .padding(.top, 6)
                .transition(.opacity)
            }

            if case .error(let msg) = nlState {
                HStack {
                    Image(systemName: "exclamationmark.circle")
                        .foregroundStyle(.red)
                    Text(msg)
                        .font(.system(size: 11))
                        .foregroundStyle(.secondary)
                    Spacer()
                    Button("Dismiss") { nlState = .idle }
                        .font(.system(size: 11))
                }
                .padding(.horizontal, 12)
                .padding(.top, 6)
                .transition(.opacity)
            }

            Divider()

            HStack(spacing: 8) {
                Picker("Mode", selection: $mode) {
                    ForEach(InputMode.allCases, id: \.self) { m in
                        Text(m.rawValue).tag(m)
                    }
                }
                .pickerStyle(.segmented)
                .frame(width: 130)
                .help("Run: execute command directly. Ask AI: translate your request to a command first.")

                ZStack(alignment: .leading) {
                    if inputText.isEmpty {
                        Text(mode == .run ? "Type a command…" : "Describe what you want to do…")
                            .font(.system(size: 13, design: mode == .run ? .monospaced : .default))
                            .foregroundStyle(.tertiary)
                            .allowsHitTesting(false)
                    }

                    TextField("", text: $inputText)
                        .font(.system(size: 13, design: mode == .run ? .monospaced : .default))
                        .textFieldStyle(.plain)
                        .focused($isFocused)
                        .onSubmit { submit() }
                }

                if case .thinking = nlState {
                    ProgressView()
                        .scaleEffect(0.7)
                        .frame(width: 28, height: 28)
                } else {
                    Button(action: submit) {
                        Image(systemName: mode == .run ? "return" : "sparkles")
                            .font(.system(size: 13, weight: .medium))
                            .foregroundStyle(inputText.isEmpty ? Color.secondary : Color.white)
                    }
                    .buttonStyle(SendButtonStyle(disabled: inputText.isEmpty))
                    .disabled(inputText.isEmpty)
                    .keyboardShortcut(.return, modifiers: [])
                }
            }
            .padding(.horizontal, 12)
            .padding(.vertical, 8)
        }
        .background(.bar)
        .animation(.easeInOut(duration: 0.2), value: nlState)
        .onAppear { isFocused = true }
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
                            nlState = .suggestion(suggestion)
                        }
                    case .failure(let error):
                        withAnimation {
                            if let aiError = error as? AIError, case .unavailable = aiError {
                                nlState = .unavailable
                            } else {
                                nlState = .error(error.localizedDescription)
                            }
                        }
                    }
                }
            }
        }
    }
}

private struct SendButtonStyle: ButtonStyle {
    let disabled: Bool

    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .frame(width: 28, height: 28)
            .background(
                Circle()
                    .fill(disabled ? Color.secondary.opacity(0.2) : Color.accentColor)
            )
            .opacity(configuration.isPressed ? 0.7 : 1)
    }
}

#Preview {
    NLCommandBarView()
        .environment(SessionState())
        .frame(width: 700)
}
