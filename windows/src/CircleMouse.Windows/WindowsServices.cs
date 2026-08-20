using System.Diagnostics;
using System.Runtime.InteropServices;
using CircleMouse.Core;

namespace CircleMouse.Windows;

public sealed class ForegroundApplication
{
    public string? GetExecutableName()
    {
        var window = GetForegroundWindow();
        _ = GetWindowThreadProcessId(window, out var processId);
        try { return Process.GetProcessById((int)processId).MainModule?.FileName; }
        catch { return null; }
    }
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint window, out uint processId);
}

public sealed class SendInputActionExecutor
{
    private const uint InputKeyboard = 1, KeyUp = 2;

    public bool Execute(ActionDefinition action)
    {
        if (action is not KeyboardShortcutAction shortcut || shortcut.Keys.Count == 0) return false;
        var inputs = shortcut.Keys.Select(key => Keyboard(key, 0))
            .Concat(shortcut.Keys.Reverse().Select(key => Keyboard(key, KeyUp))).ToArray();
        return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>()) == (uint)inputs.Length;
    }

    private static Input Keyboard(VirtualKey key, uint flags) => new() { Type = InputKeyboard, Union = new InputUnion { Keyboard = new KeyboardInput { VirtualKey = (ushort)key, Flags = flags } } };
    [StructLayout(LayoutKind.Sequential)] private struct Input { public uint Type; public InputUnion Union; }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion { [FieldOffset(0)] public KeyboardInput Keyboard; }
    [StructLayout(LayoutKind.Sequential)] private struct KeyboardInput { public ushort VirtualKey; public ushort ScanCode; public uint Flags; public uint Time; public nuint ExtraInfo; }
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint count, Input[] inputs, int size);
}
