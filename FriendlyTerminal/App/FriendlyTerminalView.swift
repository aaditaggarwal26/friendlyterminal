import AppKit
import SwiftTerm

final class FriendlyTerminalView: LocalProcessTerminalView {

    override init(frame: NSRect) {
        super.init(frame: frame)
        setupClickRecognizer()
    }

    @available(*, unavailable)
    required init?(coder: NSCoder) { fatalError("init(coder:) not used") }

    private func setupClickRecognizer() {
        let recognizer = NSClickGestureRecognizer(target: self, action: #selector(handleClick(_:)))
        recognizer.numberOfClicksRequired = 1
        recognizer.delaysPrimaryMouseButtonEvents = false
        addGestureRecognizer(recognizer)
    }

    @objc private func handleClick(_ recognizer: NSClickGestureRecognizer) {
        let point = recognizer.location(in: self)
        moveReadlineCursorTo(clickPoint: point)
    }

    private func moveReadlineCursorTo(clickPoint: NSPoint) {
        let charWidth = computeCharWidth()
        guard charWidth > 0 else { return }

        let targetCol = max(0, Int(clickPoint.x / charWidth))

        let terminal = getTerminal()
        let currentCol = terminal.buffer.x

        let delta = targetCol - currentCol
        guard delta != 0 else { return }

        let arrowSeq = delta > 0 ? "\u{1B}[C" : "\u{1B}[D"
        let count = min(abs(delta), 500)
        send(txt: String(repeating: arrowSeq, count: count))
    }

    private func computeCharWidth() -> CGFloat {
        let attrs: [NSAttributedString.Key: Any] = [.font: self.font as Any]
        let width = ("M" as NSString).size(withAttributes: attrs).width
        return width > 0 ? width : 8.0
    }
}
