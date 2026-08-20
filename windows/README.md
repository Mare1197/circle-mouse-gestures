# Circle Mouse for Windows

This directory contains the from-scratch native Windows implementation and its first working vertical slice. It does not use the browser extension or XMBC code.

## Build and run

On Windows 11 with the .NET 8 SDK:

```powershell
dotnet restore CircleMouse.sln
dotnet test CircleMouse.sln
dotnet run --project src/CircleMouse.App
```

The tray app creates a human-readable configuration on first launch. By default, pressing Mouse Button 4 (XButton1) sends Ctrl+Shift+T system-wide. Right-click the tray icon to pause mappings or exit. An application profile with an `executable` such as `chrome.exe` overrides the global mapping.

See [the architecture and phased implementation plan](docs/ARCHITECTURE.md) before adding a subsystem.
