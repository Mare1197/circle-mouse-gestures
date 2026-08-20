using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CircleMouse.Core;

namespace CircleMouse.Windows;

public sealed class LowLevelMouseHook : IDisposable
{
    private const int WhMouseLl = 14;
    private const int WmXButtonDown = 0x020B;
    private const uint XButton1 = 1;
    private readonly HookProcedure procedure;
    private readonly Func<MouseTrigger, bool> handler;
    private nint hook;

    public LowLevelMouseHook(Func<MouseTrigger, bool> handler)
    {
        this.handler = handler;
        procedure = Callback;
    }

    public void Start()
    {
        if (hook != 0) return;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule!;
        hook = SetWindowsHookEx(WhMouseLl, procedure, GetModuleHandle(module.ModuleName), 0);
        if (hook == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private nint Callback(int code, nint message, nint data)
    {
        if (code >= 0 && message == WmXButtonDown)
        {
            var input = Marshal.PtrToStructure<MouseHookData>(data);
            var button = (input.MouseData >> 16) & 0xffff;
            if (button == XButton1 && handler(MouseTrigger.XButton1)) return 1;
        }
        return CallNextHookEx(hook, code, message, data);
    }

    public void Dispose()
    {
        if (hook != 0) UnhookWindowsHookEx(hook);
        hook = 0;
        GC.SuppressFinalize(this);
    }

    private delegate nint HookProcedure(int code, nint message, nint data);
    [StructLayout(LayoutKind.Sequential)] private struct MouseHookData { public Point Point; public uint MouseData; public uint Flags; public uint Time; public nuint ExtraInfo; }
    [StructLayout(LayoutKind.Sequential)] private struct Point { public int X; public int Y; }
    [DllImport("user32.dll", SetLastError = true)] private static extern nint SetWindowsHookEx(int id, HookProcedure callback, nint module, uint threadId);
    [DllImport("user32.dll")] private static extern nint CallNextHookEx(nint hook, int code, nint message, nint data);
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)] private static extern bool UnhookWindowsHookEx(nint hook);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern nint GetModuleHandle(string? name);
}
