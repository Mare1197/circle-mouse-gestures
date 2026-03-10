# Windows desktop build

This folder adds a minimal Electron wrapper so the extension UI can be bundled as a Windows-native desktop application.

## Prerequisites
- Node.js 18+
- Windows build tools available in your environment (for signing/installer generation if needed)

## Usage
1. Build the extension assets so the desktop shell can load them:
   ```bash
   npm run build
   ```
2. Start the desktop shell in development mode (opens the options UI):
   ```bash
   npm run start:desktop
   ```
3. Produce a Windows installer (creates `release/` with an NSIS installer):
   ```bash
   npm run build:desktop
   ```

The Electron window currently loads `dist/options/index.html`, which mirrors the extension's options interface. You can extend `desktop/main.js` and `desktop/preload.js` if you need deeper integration with native Windows APIs (tray icons, global shortcuts, etc.).
