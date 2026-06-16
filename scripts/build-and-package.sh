#!/usr/bin/env bash
#
# Builds a Release FriendlyTerminal.app and packages it into a drag-to-install
# .dmg. Used by the GitHub Actions release workflow, but you can also run it
# locally (you need Xcode and `brew install xcodegen`):
#
#     ./scripts/build-and-package.sh
#
# The finished disk image is written to build/FriendlyTerminal.dmg.
#
set -euo pipefail

APP_NAME="FriendlyTerminal"
SCHEME="FriendlyTerminal"
BUILD_DIR="build"
DERIVED="$BUILD_DIR/DerivedData"
DMG_PATH="$BUILD_DIR/${APP_NAME}.dmg"

cd "$(dirname "$0")/.."

# 1. Generate the Xcode project from project.yml (it isn't checked into git).
xcodegen generate

# 2. Build a Release app. We ad-hoc sign it ("-") so it runs on any Mac without
#    an Apple Developer account; users just right-click -> Open the first time.
#    We build arm64 (Apple Silicon) only: the app requires macOS 15+ with AI
#    features gated to macOS 26, so the audience is Apple Silicon, and the Intel
#    slice of a universal build doesn't cross-compile cleanly on current Xcode.
xcodebuild \
  -project "${APP_NAME}.xcodeproj" \
  -scheme "$SCHEME" \
  -configuration Release \
  -destination 'platform=macOS,arch=arm64' \
  -derivedDataPath "$DERIVED" \
  ARCHS=arm64 \
  ONLY_ACTIVE_ARCH=NO \
  CODE_SIGN_STYLE=Manual \
  CODE_SIGN_IDENTITY="-" \
  DEVELOPMENT_TEAM="" \
  build

APP_PATH="$DERIVED/Build/Products/Release/${APP_NAME}.app"
if [ ! -d "$APP_PATH" ]; then
  echo "error: build did not produce $APP_PATH" >&2
  exit 1
fi

# 3. Stage a folder containing the app plus a shortcut to /Applications, so the
#    mounted disk image shows the familiar "drag me into Applications" layout.
STAGE="$BUILD_DIR/dmg-stage"
rm -rf "$STAGE" "$DMG_PATH"
mkdir -p "$STAGE"
cp -R "$APP_PATH" "$STAGE/"
ln -s /Applications "$STAGE/Applications"

# 4. Build a compressed disk image from the staged folder.
hdiutil create \
  -volname "$APP_NAME" \
  -srcfolder "$STAGE" \
  -ov -format UDZO \
  "$DMG_PATH"

echo "Created $DMG_PATH"
